using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Lighting;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering.Pipeline;

public abstract class RenderPassNode
{
    private RenderPassNode? _child;
    protected RenderPipeline Pipeline = null!;
    
    
    public void SetChild(RenderPassNode child)
    {
        _child = child;
    }
    
    
    public void Prepare(RenderPipeline pipeline)
    {
        Pipeline = pipeline;
        OnPrepare(pipeline.Width, pipeline.Height);
        
        _child?.Prepare(pipeline);
    }


    public RenderTexture? Evaluate(RenderTexture? source)
    {
        RenderTexture? result = Render(source);
        
        if (_child != null)
            return _child.Evaluate(result);
        
        return result;
    }


    protected virtual void OnEnable() { }
    protected virtual void OnPrepare(int width, int height) { }
    
    
    protected abstract RenderTexture? Render(RenderTexture? source);

    
    protected RenderTexture GetRenderTexture(float scale, TextureImageFormat[] format)
    {
        RenderTexture rt = RenderTexture.GetTemporaryRT((int)(Pipeline.Width * scale), (int)(Pipeline.Height * scale), format);
        Pipeline.UsedRenderTextures.Add(rt);
        return rt;
    }

    
    protected void ReleaseRenderTexture(RenderTexture rt)
    {
        Pipeline.UsedRenderTextures.Remove(rt);
        RenderTexture.ReleaseTemporaryRT(rt);
    }
}

public class PBRDeferredNode : RenderPassNode
{
    public TextureImageFormat Format = TextureImageFormat.RGB_16_S;

    public float Scale = 1.0f;

    
    protected override RenderTexture Render(RenderTexture? source)
    {
        RenderTexture result = GetRenderTexture(Scale, [Format]);
        
        result.Begin();
        
        Graphics.Clear();
        Graphics.RenderingCamera!.RenderAllOfOrder(ComponentRenderOrder.Lighting);
        
        result.End();
        
        return result;
    }
}

public class PostPBRDeferredNode : RenderPassNode
{
    private Material? _combineShader;
    
    
    protected override RenderTexture? Render(RenderTexture? source)
    {
        GBuffer gBuffer = CameraComponent.RenderingCamera.GBuffer!;
        
        if (source == null)
            return null;

        _combineShader ??= new Material(Shader.Find("Defaults/GBuffercombine.shader"));
        _combineShader.SetTexture("gAlbedoAO", gBuffer.AlbedoAO);
        _combineShader.SetTexture("gLighting", source.InternalTextures[0]);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _combineShader, 0, true);
        ReleaseRenderTexture(source);
        
        return result;
    }
}

public class ProceduralSkyboxNode : RenderPassNode
{
    public float FogDensity = 0.08f;
    private Material? _mat;
    
    
    protected override RenderTexture? Render(RenderTexture? source)
    {
        CameraComponent camera = CameraComponent.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;
        
        if (source == null)
            return null;

        _mat ??= new Material(Shader.Find("Defaults/ProceduralSkybox.shader"));
        _mat.SetTexture("gColor", source.InternalTextures[0]);
        _mat.SetTexture("gPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetFloat("fogDensity", FogDensity);

        // Find DirectionalLight
        DirectionalLight? light = camera.Entity.Scene.FindObjectOfType<DirectionalLight>();
        if (light != null)
            _mat.SetVector("uSunPos", -light.Entity.Transform.Forward);
        else // Fallback to a reasonable default
            _mat.SetVector("uSunPos", new Vector3(0.5f, 0.5f, 0.5f));

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);
        
        return result;
    }
}
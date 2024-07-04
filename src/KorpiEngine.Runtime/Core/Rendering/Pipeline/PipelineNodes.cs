using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
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


    protected virtual void OnEnable()
    {
    }


    protected virtual void OnPrepare(int width, int height)
    {
    }


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
        CameraComponent.RenderingCamera.RenderLights();

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

        _combineShader ??= new Material(Shader.Find("Defaults/GBufferCombine.shader"), "G-buffer combine material");
        _combineShader.SetTexture("_GAlbedoAO", gBuffer.AlbedoAO);
        _combineShader.SetTexture("_GLighting", source.InternalTextures[0]);

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

        _mat ??= new Material(Shader.Find("Defaults/ProceduralSkybox.shader"), "procedural skybox material");
        _mat.SetTexture("_GColor", source.InternalTextures[0]);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetFloat("_FogDensity", FogDensity);

        // Find DirectionalLight
        DirectionalLight? light = camera.Entity.Scene.FindObjectOfType<DirectionalLight>();
        if (light != null)
            _mat.SetVector("_SunPos", -light.Entity.Transform.Forward);
        else // Fallback to a reasonable default
            _mat.SetVector("_SunPos", new Vector3(0.5f, 0.5f, 0.5f));

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class ScreenSpaceReflectionNode : RenderPassNode
{
    public float Threshold = 0.15f;
    public int Steps = 16;
    public int RefineSteps = 4;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        CameraComponent camera = CameraComponent.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        _mat ??= new Material(Shader.Find("Defaults/SSR.shader"), "SSR material");
        _mat.SetTexture("_GColor", source.InternalTextures[0]);
        _mat.SetTexture("_GNormalMetallic", gBuffer.NormalMetallic);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetFloat("SSRThreshold", Math.Clamp(Threshold, 0.0f, 1.0f));
        _mat.SetInt("SSRSteps", Math.Clamp(Steps, 16, 32));
        _mat.SetInt("SSRBisteps", Math.Clamp(RefineSteps, 0, 16));

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class TAANode : RenderPassNode
{
    public bool Jitter2X = false;

    private Material? _mat;
    private Vector2 _jitter;
    private Vector2 _previousJitter;

    private static readonly Vector2[] Halton16 =
    [
        new Vector2(0.5f, 0.333333f),
        new Vector2(0.25f, 0.666667f),
        new Vector2(0.75f, 0.111111f),
        new Vector2(0.125f, 0.444444f),
        new Vector2(0.625f, 0.777778f),
        new Vector2(0.375f, 0.222222f),
        new Vector2(0.875f, 0.555556f),
        new Vector2(0.0625f, 0.888889f),
        new Vector2(0.5625f, 0.037037f),
        new Vector2(0.3125f, 0.370370f),
        new Vector2(0.8125f, 0.703704f),
        new Vector2(0.1875f, 0.148148f),
        new Vector2(0.6875f, 0.481481f),
        new Vector2(0.4375f, 0.814815f),
        new Vector2(0.9375f, 0.259259f),
        new Vector2(0.03125f, 0.592593f)
    ];


    protected override void OnPrepare(int width, int height)
    {
        // Apply Halton jitter
        long n = Time.TotalFrameCount % 16;
        Vector2 halton = Halton16[n];
        _previousJitter = _jitter;
        _jitter = new Vector2(halton.X - 0.5f, halton.Y - 0.5f) * 2.0;
        if (Jitter2X)
            _jitter *= 2.0;

        Graphics.ProjectionMatrix.M31 += _jitter.X / width;
        Graphics.ProjectionMatrix.M32 += _jitter.Y / height;

        Graphics.UseJitter = true; // This applies the jitter to the Velocity Buffer/Motion Vectors
        Graphics.Jitter = _jitter / new Vector2(width, height);
        Graphics.PreviousJitter = _previousJitter / new Vector2(width, height);
    }


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        CameraComponent camera = CameraComponent.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        RenderTexture history = camera.GetCachedRT("TAA_HISTORY", Pipeline.Width, Pipeline.Height, [TextureImageFormat.RGB_16_S]);

        _mat ??= new Material(Shader.Find("Defaults/TAA.shader"), "TAA material");
        _mat.SetTexture("_GColor", source.InternalTextures[0]);
        _mat.SetTexture("_GHistory", history.InternalTextures[0]);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetTexture("_GVelocity", gBuffer.Velocity);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetInt("_ClampRadius", Jitter2X ? 2 : 1);

        _mat.SetVector("_Jitter", Graphics.Jitter);
        _mat.SetVector("_PreviousJitter", Graphics.PreviousJitter);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _mat, 0, true);
        Graphics.Blit(history, result.InternalTextures[0], true);

        return result;
    }
}

public class DepthOfFieldNode : RenderPassNode
{
    public float FocusStrength = 150f;
    public float Quality = 0.05f;
    public int BlurRadius = 5;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        CameraComponent camera = CameraComponent.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        _mat ??= new Material(Shader.Find("Defaults/DOF.shader"), "DOF material");
        _mat.SetTexture("_GCombined", source.InternalTextures[0]);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetFloat("_Quality", Math.Clamp(Quality, 0.0f, 0.9f));
        _mat.SetFloat("_BlurRadius", Math.Clamp(BlurRadius, 2, 40));
        _mat.SetFloat("_FocusStrength", FocusStrength);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class BloomNode : RenderPassNode
{
    public float Radius = 10f;
    public float Threshold = 0.5f;
    public int Passes = 10;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        _mat ??= new Material(Shader.Find("Defaults/Bloom.shader"), "bloom material");

        RenderTexture front = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        RenderTexture back = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        RenderTexture[] rts = [front, back];

        _mat.SetFloat("_Alpha", 1.0f);
        _mat.SetTexture("_GColor", source.InternalTextures[0]);
        _mat.SetFloat("_Radius", 1.5f);
        _mat.SetFloat("_Threshold", Math.Clamp(Threshold, 0.0f, 8f));
        Graphics.Blit(rts[0], _mat, 0, true);
        Graphics.Blit(rts[1], _mat, 0, true);
        _mat.SetFloat("_Threshold", 0.0f);

        for (int i = 1; i <= Passes; i++)
        {
            _mat.SetFloat("_Alpha", 1.0f);
            _mat.SetTexture("_GColor", rts[0].InternalTextures[0]);
            _mat.SetFloat("_Radius", Math.Clamp(Radius, 0.0f, 32f) + i);
            Graphics.Blit(rts[1], _mat, 0, false);

            (rts[1], rts[0]) = (rts[0], rts[1]);
        }

        // Final pass
        Graphics.Blit(rts[0], source.InternalTextures[0], false);
        ReleaseRenderTexture(rts[1]);
        ReleaseRenderTexture(source);

        return rts[0];
    }
}

public class TonemappingNode : RenderPassNode
{
    public float Contrast = 1.05f;
    public float Saturation = 1.15f;

    public enum TonemapperType
    {
        Melon,
        Aces,
        Reinhard,
        Uncharted2,
        Filmic,
        None
    }

    public TonemapperType UsedTonemapperType = TonemapperType.Melon;
    public bool UseGammaCorrection = true;

    private Material? _acesMat;
    private TonemapperType? _prevTonemapper;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        _acesMat ??= new Material(Shader.Find("Defaults/Tonemapper.shader"), "tonemapping material");
        _acesMat.SetTexture("_GAlbedo", source.InternalTextures[0]);
        _acesMat.SetFloat("_Contrast", Math.Clamp(Contrast, 0, 2));
        _acesMat.SetFloat("_Saturation", Math.Clamp(Saturation, 0, 2));

        // Because we always Reset the tonemappers to disabled then re-enable them
        // this will trigger a Uniform Location Cache clear every single frame
        // As the shader could be changing, so we do a previous check to see if we need to do this
        if (_prevTonemapper != UsedTonemapperType)
        {
            _prevTonemapper = UsedTonemapperType;
            _acesMat.DisableKeyword("MELON");
            _acesMat.DisableKeyword("ACES");
            _acesMat.DisableKeyword("REINHARD");
            _acesMat.DisableKeyword("UNCHARTED");
            _acesMat.DisableKeyword("FILMIC");

            switch (UsedTonemapperType)
            {
                case TonemapperType.Melon:
                    _acesMat.EnableKeyword("MELON");
                    break;
                case TonemapperType.Aces:
                    _acesMat.EnableKeyword("ACES");
                    break;
                case TonemapperType.Reinhard:
                    _acesMat.EnableKeyword("REINHARD");
                    break;
                case TonemapperType.Uncharted2:
                    _acesMat.EnableKeyword("UNCHARTED");
                    break;
                case TonemapperType.Filmic:
                    _acesMat.EnableKeyword("FILMIC");
                    break;
            }
        }

        if (UseGammaCorrection)
            _acesMat.EnableKeyword("GAMMACORRECTION");
        else
            _acesMat.DisableKeyword("GAMMACORRECTION");

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_S]);
        Graphics.Blit(result, _acesMat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}
using KorpiEngine.Mathematics;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

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

public class LightingPassNode : RenderPassNode
{
    public TextureImageFormat Format { get; set; } = TextureImageFormat.RGB_16_F;

    public float Scale { get; set; } = 1.0f;


    protected override RenderTexture Render(RenderTexture? _)
    {
        RenderTexture lightingTex = GetRenderTexture(Scale, [Format]);

        lightingTex.Begin();

        Graphics.Clear();
        Camera.RenderingCamera.RenderLights();

        lightingTex.End();

        return lightingTex;
    }
}

public class LightingCombinePassNode : RenderPassNode
{
    private Material? _combineShader;
    
    
    /// <param name="source">Lighting Texture</param>
    /// <returns>A texture with geometry and lighting combined</returns>
    protected override RenderTexture? Render(RenderTexture? source)
    {
        GBuffer gBuffer = Camera.RenderingCamera.GBuffer!;

        if (source == null)
            return null;

        _combineShader ??= new Material(Shader.Find("Assets/Defaults/GBufferCombine.kshader"), "G-buffer combine material", false);
        _combineShader.SetTexture("_GAlbedoAO", gBuffer.AlbedoAO);
        _combineShader.SetTexture("_GLighting", source.MainTexture);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _combineShader, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class UnlitCombinePassNode : RenderPassNode
{
    protected override RenderTexture? Render(RenderTexture? source)
    {
        GBuffer gBuffer = Camera.RenderingCamera.GBuffer!;

        if (source == null)
            return null;

        Graphics.Blit(source, gBuffer.Unlit, false);

        return source;
    }
}

public class ProceduralSkyboxNode : RenderPassNode
{
    public float FogDensity { get; set; } = 0.08f;
    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        Camera camera = Camera.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        if (source == null)
            return null;

        _mat ??= new Material(Shader.Find("Assets/Defaults/ProceduralSkybox.kshader"), "procedural skybox material", false);
        _mat.SetTexture("_GColor", source.MainTexture);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetFloat("_FogDensity", FogDensity);

        // Find DirectionalLight
        DirectionalLight? light = camera.Entity.Scene!.FindObjectOfType<DirectionalLight>();
        if (light != null)
            _mat.SetVector("_SunPos", -light.Entity.Transform.Forward);
        else // Fallback to a reasonable default
            _mat.SetVector("_SunPos", new Vector3(0.5f, 0.5f, 0.5f));

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class ScreenSpaceReflectionNode : RenderPassNode
{
    public float Threshold { get; set; } = 0.15f;
    public int Steps { get; set; } = 16;
    public int RefineSteps { get; set; } = 4;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        Camera camera = Camera.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        _mat ??= new Material(Shader.Find("Assets/Defaults/SSR.kshader"), "SSR material", false);
        _mat.SetTexture("_GColor", source.MainTexture);
        _mat.SetTexture("_GNormalMetallic", gBuffer.NormalMetallic);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetFloat("_SSRThreshold", Math.Clamp(Threshold, 0.0f, 1.0f));
        _mat.SetInt("_SSRSteps", Math.Clamp(Steps, 16, 32));
        _mat.SetInt("_SSRBisteps", Math.Clamp(RefineSteps, 0, 16));

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class TAANode : RenderPassNode
{
    public bool Jitter2X { get; set; } = false;

    private Material? _mat;
    private Vector2 _jitter;

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
        Vector2 previousJitter = _jitter;
        _jitter = new Vector2(halton.X - 0.5f, halton.Y - 0.5f) * 2.0f;
        if (Jitter2X)
            _jitter *= 2.0f;

        Matrix4x4 proj = Graphics.ProjectionMatrix;
        proj.M31 += _jitter.X / width;
        proj.M32 += _jitter.Y / height;
        Graphics.ProjectionMatrix = proj;

        Graphics.UseJitter = true; // This applies the jitter to the Velocity Buffer/Motion Vectors
        Graphics.Jitter = _jitter / new Vector2(width, height);
        Graphics.PreviousJitter = previousJitter / new Vector2(width, height);
    }


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        Camera camera = Camera.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        RenderTexture history = camera.GetCachedRT("TAA_HISTORY", Pipeline.Width, Pipeline.Height, [TextureImageFormat.RGB_16_F]);

        _mat ??= new Material(Shader.Find("Assets/Defaults/TAA.kshader"), "TAA material", false);
        _mat.SetTexture("_GColor", source.MainTexture);
        _mat.SetTexture("_GHistory", history.MainTexture);
        _mat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);
        _mat.SetTexture("_GVelocity", gBuffer.Velocity);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetInt("_ClampRadius", Jitter2X ? 2 : 1);

        _mat.SetVector("_Jitter", Graphics.Jitter);
        _mat.SetVector("_PreviousJitter", Graphics.PreviousJitter);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _mat, 0, true);
        Graphics.Blit(history, result.MainTexture, true);

        return result;
    }
}

public class DepthOfFieldNode : RenderPassNode
{
    public float FocusStrength { get; set; } = 150f;
    public float Quality { get; set; } = 0.05f;
    public int BlurRadius { get; set; } = 5;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        Camera camera = Camera.RenderingCamera;
        GBuffer gBuffer = camera.GBuffer!;

        _mat ??= new Material(Shader.Find("Assets/Defaults/DOF.kshader"), "DOF material", false);
        _mat.SetTexture("_GCombined", source.MainTexture);
        _mat.SetTexture("_GDepth", gBuffer.Depth!);

        _mat.SetFloat("_Quality", Math.Clamp(Quality, 0.0f, 0.9f));
        _mat.SetFloat("_BlurRadius", Math.Clamp(BlurRadius, 2, 40));
        _mat.SetFloat("_FocusStrength", FocusStrength);

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _mat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}

public class BloomNode : RenderPassNode
{
    public float Radius { get; set; } = 10f;
    public float Threshold { get; set; } = 0.5f;
    public int Passes { get; set; } = 10;

    private Material? _mat;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        _mat ??= new Material(Shader.Find("Assets/Defaults/Bloom.kshader"), "bloom material", false);

        RenderTexture front = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        RenderTexture back = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        RenderTexture[] rts = [front, back];

        _mat.SetFloat("_Alpha", 1.0f);
        _mat.SetTexture("_GColor", source.MainTexture);
        _mat.SetFloat("_Radius", 1.5f);
        _mat.SetFloat("_Threshold", Math.Clamp(Threshold, 0.0f, 8f));
        Graphics.Blit(rts[0], _mat, 0, true);
        Graphics.Blit(rts[1], _mat, 0, true);
        _mat.SetFloat("_Threshold", 0.0f);

        for (int i = 1; i <= Passes; i++)
        {
            _mat.SetFloat("_Alpha", 1.0f);
            _mat.SetTexture("_GColor", rts[0].MainTexture);
            _mat.SetFloat("_Radius", Math.Clamp(Radius, 0.0f, 32f) + i);
            Graphics.Blit(rts[1], _mat, 0, false);

            (rts[1], rts[0]) = (rts[0], rts[1]);
        }

        // Final pass
        Graphics.Blit(rts[0], source.MainTexture, false);
        ReleaseRenderTexture(rts[1]);
        ReleaseRenderTexture(source);

        return rts[0];
    }
}

public class TonemappingNode : RenderPassNode
{
    public float Contrast { get; set; } = 1.05f;
    public float Saturation { get; set; } = 1.15f;

    public enum TonemapperType
    {
        Melon,
        Aces,
        Reinhard,
        Uncharted2,
        Filmic,
        None
    }

    public TonemapperType UsedTonemapperType { get; set; } = TonemapperType.Melon;
    public bool UseGammaCorrection { get; set; } = true;

    private Material? _acesMat;
    private TonemapperType? _prevTonemapper;


    protected override RenderTexture? Render(RenderTexture? source)
    {
        if (source == null)
            return null;

        _acesMat ??= new Material(Shader.Find("Assets/Defaults/Tonemapper.kshader"), "tonemapping material", false);
        _acesMat.SetTexture("_GAlbedo", source.MainTexture);
        _acesMat.SetFloat("_Contrast", Math.Clamp(Contrast, 0, 2));
        _acesMat.SetFloat("_Saturation", Math.Clamp(Saturation, 0, 2));

        // Because we always Reset the tone mappers to disabled and then re-enable them,
        // this will trigger a Uniform Location Cache clear every single frame.
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

        RenderTexture result = GetRenderTexture(1f, [TextureImageFormat.RGB_16_F]);
        Graphics.Blit(result, _acesMat, 0, true);
        ReleaseRenderTexture(source);

        return result;
    }
}
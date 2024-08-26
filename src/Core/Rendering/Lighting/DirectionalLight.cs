using ImGuiNET;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.UI.DearImGui;

namespace KorpiEngine.Core.Rendering.Lighting;

public sealed class DirectionalLight : EntityComponent
{
    public enum Resolution
    {
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }
    
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public Resolution ShadowResolution
    {
        get => _shadowResolution;
        set
        {
            _shadowResolution = value;
            InvalidateShadowmap();
        }
    }

    public Color Color { get; set; } = Color.White;
    public float Intensity { get; set; } = 8f;
    public int QualitySamples { get; set; } = 64;
    public int BlockerSamples { get; set; } = 64;
    public bool CastShadows { get; set; } = true;
    public float ShadowDistance { get; set; } = 50f;
    public float ShadowRadius { get; set; } = 0.035f;
    public float ShadowPenumbra { get; set; } = 80f;
    public float ShadowMinimumPenumbra { get; set; } = 0.1f;
    public float ShadowBias { get; set; } = 0.001f;
    public float ShadowNormalBias { get; set; } = 0.1f;

    private Resolution _shadowResolution = Resolution._2048;
    private Material? _lightMat;
    private RenderTexture? _shadowMap;
    private Matrix4x4 _depthMVP;

    
    protected override void OnStart()
    {
#if TOOLS
        ImGuiWindowManager.RegisterWindow(new DirectionalLightEditor(this));
#endif
    }


    protected override void OnPreRender()
    {
        UpdateShadowmap();
    }


    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Assets/Defaults/DirectionalLight.kshader"), "directional light material", false);
        _lightMat.SetVector("_LightDirection", Vector3.TransformNormal(Entity.Transform.Forward, Graphics.ViewMatrix));
        _lightMat.SetColor("_LightColor", Color);
        _lightMat.SetFloat("_LightIntensity", Intensity);

        _lightMat.SetTexture("_GAlbedoAO", Camera.RenderingCamera.GBuffer!.AlbedoAO);
        _lightMat.SetTexture("_GNormalMetallic", Camera.RenderingCamera.GBuffer.NormalMetallic);
        _lightMat.SetTexture("_GPositionRoughness", Camera.RenderingCamera.GBuffer.PositionRoughness);
        
        _lightMat.SetKeyword("CASTSHADOWS", CastShadows);

        if (CastShadows)
        {
            _lightMat.SetTexture("_ShadowMap", _shadowMap!.InternalDepth!);

            _lightMat.SetMatrix("_MatCamViewInverse", Graphics.InverseViewMatrix);
            _lightMat.SetMatrix("_MatShadowView", Graphics.DepthViewMatrix);
            _lightMat.SetMatrix("_MatShadowSpace", _depthMVP);

            _lightMat.SetFloat("_Radius", ShadowRadius);
            _lightMat.SetFloat("_Penumbra", ShadowPenumbra);
            _lightMat.SetFloat("_MinimumPenumbra", ShadowMinimumPenumbra);
            _lightMat.SetInt("_QualitySamples", QualitySamples);
            _lightMat.SetInt("_BlockerSamples", BlockerSamples);
            _lightMat.SetFloat("_Bias", ShadowBias);
            _lightMat.SetFloat("_NormalBias", ShadowNormalBias);
        }

        Graphics.Blit(_lightMat);
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.Matrix = Entity.Transform.LocalToWorldMatrix;
        Gizmos.Color = Color.Yellow;
        Gizmos.DrawDirectionalLight(Vector3.Zero);
    }


    private void UpdateShadowmap()
    {
        // Populate Shadowmap
        if (CastShadows)
        {
            int res = (int)ShadowResolution;
            _shadowMap ??= new RenderTexture(res, res, 0);

            // Compute the MVP matrix from the light's point of view
            Graphics.DepthProjectionMatrix = Matrix4x4.CreateOrthographic(ShadowDistance, ShadowDistance, 0, ShadowDistance*2);

            Vector3 forward = Entity.Transform.Forward;
            Graphics.DepthViewMatrix = Matrix4x4.CreateLookToLeftHanded(-forward * ShadowDistance, -forward, Entity.Transform.Up);

            _depthMVP = Matrix4x4.Identity;
            _depthMVP = Matrix4x4.Multiply(_depthMVP, Graphics.DepthViewMatrix);
            _depthMVP = Matrix4x4.Multiply(_depthMVP, Graphics.DepthProjectionMatrix);

            _shadowMap.Begin();
            
            Graphics.Clear(1, 1, 1, 1);
            Camera.RenderingCamera.RenderDepthGeometry();
            
            _shadowMap.End();
        }
        else
        {
            InvalidateShadowmap();
        }
    }


    private void InvalidateShadowmap()
    {
        _lightMat?.SetTexture("_ShadowMap", null);
        _shadowMap?.DestroyImmediate();
        _shadowMap = null;
    }
}
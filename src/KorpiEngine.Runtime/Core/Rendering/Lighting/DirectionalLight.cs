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
        ImGuiWindowManager.RegisterWindow(new DirectionalLightEditor(this));
    }


    protected override void OnPreRender()
    {
        UpdateShadowmap();
    }


    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Defaults/DirectionalLight.kshader"), "directional light material", false);
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

internal class DirectionalLightEditor(DirectionalLight target) : EntityComponentEditor(target)
{
    protected override void DrawEditor()
    {
        ImGui.Text("Orientation");
        System.Numerics.Vector3 forward = new((float)target.Entity.Transform.Forward.X, (float)target.Entity.Transform.Forward.Y, (float)target.Entity.Transform.Forward.Z);
        if (ImGui.DragFloat3("Forward", ref forward, 0.01f))
        {
            Vector3 newForward = new Vector3(forward.X, forward.Y, forward.Z);
            target.Entity.Transform.Forward = newForward;
        }
        
        ImGui.Text("Color");
        System.Numerics.Vector4 color = new(target.Color.R, target.Color.G, target.Color.B, target.Color.A);
        if (ImGui.ColorEdit4("##Color", ref color))
            target.Color = new Color(color);
        
        float intensity = target.Intensity;
        if (ImGui.DragFloat("Intensity", ref intensity, 0.5f, 0.5f, 50f))
            target.Intensity = intensity;
        
        ImGui.Separator();
        
        bool castShadows = target.CastShadows;
        if (ImGui.Checkbox("Cast Shadows", ref castShadows))
            target.CastShadows = castShadows;

        if (target.CastShadows)
            DrawShadowSettings();
    }


    private void DrawShadowSettings()
    {
        if (ImGui.BeginCombo("Shadow Resolution", target.ShadowResolution.ToString()))
        {
            foreach (DirectionalLight.Resolution e in Enum.GetValues<DirectionalLight.Resolution>())
            {
                if (!ImGui.Selectable(e.ToString(), target.ShadowResolution.ToString() == e.ToString()))
                    continue;

                target.ShadowResolution = e;
            }

            ImGui.EndCombo();
        }

        float shadowDistance = target.ShadowDistance;
        if (ImGui.DragFloat("Shadow Distance", ref shadowDistance, 1f, 1f, 100f))
            target.ShadowDistance = shadowDistance;

        float shadowRadius = target.ShadowRadius;
        if (ImGui.DragFloat("Shadow Radius", ref shadowRadius, 0.001f, 0.001f, 0.1f))
            target.ShadowRadius = shadowRadius;

        float shadowPenumbra = target.ShadowPenumbra;
        if (ImGui.DragFloat("Shadow Penumbra", ref shadowPenumbra, 0.1f, 0.1f, 200f))
            target.ShadowPenumbra = shadowPenumbra;

        float shadowMinimumPenumbra = target.ShadowMinimumPenumbra;
        if (ImGui.DragFloat("Shadow Minimum Penumbra", ref shadowMinimumPenumbra, 0.001f, 0.001f, 0.1f))
            target.ShadowMinimumPenumbra = shadowMinimumPenumbra;

        float shadowBias = target.ShadowBias;
        if (ImGui.DragFloat("Shadow Bias", ref shadowBias, 0.001f, 0.001f, 0.1f))
            target.ShadowBias = shadowBias;

        float shadowNormalBias = target.ShadowNormalBias;
        if (ImGui.DragFloat("Shadow Normal Bias", ref shadowNormalBias, 0.001f, 0.001f, 0.1f))
            target.ShadowNormalBias = shadowNormalBias;

        int qualitySamples = target.QualitySamples;
        if (ImGui.DragInt("Quality Samples", ref qualitySamples, 1, 1, 64))
            target.QualitySamples = qualitySamples;

        int blockerSamples = target.BlockerSamples;
        if (ImGui.DragInt("Blocker Samples", ref blockerSamples, 1, 1, 64))
            target.BlockerSamples = blockerSamples;
    }
}
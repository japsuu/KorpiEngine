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
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public enum Resolution
    {
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }

    public Resolution ShadowResolution = Resolution._1024;  //TODO: Getter/setter that invalidates the shadowmap automatically.

    public Color Color = Color.Red;
    public float Intensity = 8f;
    public int QualitySamples = 16;
    public int BlockerSamples = 16;
    public float ShadowDistance = 50f;
    public float ShadowRadius = 0.02f;
    public float ShadowPenumbra = 80f;
    public float ShadowMinimumPenumbra = 0.02f;
    public float ShadowBias = 0.00004f;
    public float ShadowNormalBias = 0.02f;
    public bool CastShadows = true;

    private Material? _lightMat;
    private RenderTexture? _shadowMap;
    private Matrix4x4 _depthMVP;
    private DirectionalLightEditor _editor;

    
    protected override void OnStart()
    {
        _editor = new DirectionalLightEditor(this);
    }


    protected override void OnDestroy()
    {
        _editor.Destroy();
    }


    protected override void OnPreRender()
    {
        UpdateShadowmap();
    }


    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Defaults/DirectionalLight.kshader"), "directional light material");
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


    public void InvalidateShadowmap()
    {
        _shadowMap?.DestroyImmediate();
        _shadowMap = null;
    }
}

internal class DirectionalLightEditor(DirectionalLight target) : ImGuiWindow(true)
{
    public override string Title => "Directional Light Editor";


    protected override void DrawContent()
    {
        ImGui.Text("Color");
        System.Numerics.Vector4 color = new(target.Color.R, target.Color.G, target.Color.B, target.Color.A);
        if (ImGui.ColorEdit4("##Color", ref color))
            target.Color = new Color(color);
        
        ImGui.DragFloat("Intensity", ref target.Intensity, 0.5f, 0.5f, 50f);
        
        ImGui.Separator();
        
        ImGui.Checkbox("Cast Shadows", ref target.CastShadows);
        
        if (target.CastShadows)
        {
            if (ImGui.BeginCombo("Shadow Resolution", target.ShadowResolution.ToString()))
            {
                foreach (DirectionalLight.Resolution e in Enum.GetValues<DirectionalLight.Resolution>())
                    if (ImGui.Selectable(e.ToString(), target.ShadowResolution.ToString() == e.ToString()))
                    {
                        target.ShadowResolution = e;
                        target.InvalidateShadowmap();
                    }

                ImGui.EndCombo();
            }
            
            ImGui.DragFloat("Shadow Distance", ref target.ShadowDistance, 1f, 1f, 100f);
            ImGui.DragFloat("Shadow Radius", ref target.ShadowRadius, 0.001f, 0.001f, 0.1f);
            ImGui.DragFloat("Shadow Penumbra", ref target.ShadowPenumbra, 0.1f, 0.1f, 200f);
            ImGui.DragFloat("Shadow Minimum Penumbra", ref target.ShadowMinimumPenumbra, 0.001f, 0.001f, 0.1f);
            ImGui.DragFloat("Shadow Bias", ref target.ShadowBias, 0.00001f, 0.00001f, 0.1f);
            ImGui.DragFloat("Shadow Normal Bias", ref target.ShadowNormalBias, 0.001f, 0.001f, 0.1f);
            ImGui.DragInt("Quality Samples", ref target.QualitySamples, 1, 1, 64);
            ImGui.DragInt("Blocker Samples", ref target.BlockerSamples, 1, 1, 64);
        }
    }
}
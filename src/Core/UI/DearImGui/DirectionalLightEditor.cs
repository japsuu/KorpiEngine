#if TOOLS
using ImGuiNET;
using KorpiEngine.Core.API;
using KorpiEngine.Core.Rendering.Lighting;

namespace KorpiEngine.Core.UI.DearImGui;

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
#endif
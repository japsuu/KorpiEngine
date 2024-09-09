#if TOOLS
using ImGuiNET;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace KorpiEngine.UI.DearImGui;

internal class CameraEditor(Camera target) : EntityComponentEditor(target)
{
    protected override void DrawEditor()
    {
        ImGui.Text("Camera Settings");

        int renderPriority = target.RenderPriority;
        if (ImGui.DragInt("Render Priority", ref renderPriority, 1, short.MinValue, short.MaxValue))
            target.RenderPriority = (short)renderPriority;

        float renderResolution = target.RenderResolution;
        if (ImGui.DragFloat("Render Resolution", ref renderResolution, 0.1f, 0.1f, 10f))
            target.RenderResolution = renderResolution;

        DrawProjectionSettings();

        DrawClearSettings();

        DrawDebugSettings();
    }


    private void DrawProjectionSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Projection Settings");
        if (ImGui.BeginCombo("Projection Type", target.ProjectionType.ToString()))
        {
            foreach (CameraProjectionType type in Enum.GetValues<CameraProjectionType>())
            {
                if (ImGui.Selectable(type.ToString(), target.ProjectionType == type))
                    target.ProjectionType = type;
            }
            ImGui.EndCombo();
        }

        float fovDegrees = target.FOVDegrees;
        if (ImGui.DragFloat("FOV Degrees", ref fovDegrees, 1f, 1f, 179f))
            target.FOVDegrees = fovDegrees;

        float orthographicSize = target.OrthographicSize;
        if (ImGui.DragFloat("Orthographic Size", ref orthographicSize, 0.1f, 0.1f, 100f))
            target.OrthographicSize = orthographicSize;

        float nearClipPlane = target.NearClipPlane;
        if (ImGui.DragFloat("Near Clip Plane", ref nearClipPlane, 0.01f, 0.01f, 100f))
            target.NearClipPlane = nearClipPlane;

        float farClipPlane = target.FarClipPlane;
        if (ImGui.DragFloat("Far Clip Plane", ref farClipPlane, 1f, 1f, 10000f))
            target.FarClipPlane = farClipPlane;
    }


    private void DrawClearSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Clear Settings");
        if (ImGui.BeginCombo("Clear Type", target.ClearType.ToString()))
        {
            foreach (CameraClearType type in Enum.GetValues<CameraClearType>())
            {
                if (ImGui.Selectable(type.ToString(), target.ClearType == type))
                    target.ClearType = type;
            }
            ImGui.EndCombo();
        }

        ImGui.Text("Clear Flags");
        int clearFlags = (int)target.ClearFlags;
        if (ImGui.CheckboxFlags("Color", ref clearFlags, (int)CameraClearFlags.Color))
            target.ClearFlags = (CameraClearFlags)clearFlags;
        if (ImGui.CheckboxFlags("Depth", ref clearFlags, (int)CameraClearFlags.Depth))
            target.ClearFlags = (CameraClearFlags)clearFlags;
        if (ImGui.CheckboxFlags("Stencil", ref clearFlags, (int)CameraClearFlags.Stencil))
            target.ClearFlags = (CameraClearFlags)clearFlags;

        System.Numerics.Vector4 clearColor = new(target.ClearColor.R, target.ClearColor.G, target.ClearColor.B, target.ClearColor.A);
        if (ImGui.ColorEdit4("Clear Color", ref clearColor))
            target.ClearColor = new ColorHDR(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
    }


    private void DrawDebugSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Debug Settings");
        
        if (ImGui.BeginCombo("Debug Draw Type", target.DebugDrawType.ToString()))
        {
            foreach (CameraDebugDrawType type in Enum.GetValues<CameraDebugDrawType>())
            {
                if (ImGui.Selectable(type.ToString(), target.DebugDrawType == type))
                    target.SetDebugDrawType(type);
            }
            ImGui.EndCombo();
        }
        
        bool showGizmos = target.ShowGizmos;
        if (ImGui.Checkbox("Show Gizmos", ref showGizmos))
            target.ShowGizmos = showGizmos;
    }
}
#endif
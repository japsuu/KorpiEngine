﻿#if KORPI_TOOLS
using ImGuiNET;
using KorpiEngine.Entities;
using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace KorpiEngine.UI.DearImGui;

public class EntityEditor() : ImGuiWindow(true)
{
    private Entity? _target;

    public override string Title => "Entity Editor";

    protected override void PreUpdate()
    {
        if (!Input.GetMouseButtonDown(MouseButton.Left) || GUI.WantCaptureMouse)
            return;

        Vector2 mousePos = Input.MousePosition;
        Vector2 mouseUV = new Vector2(mousePos.X / Graphics.ViewportResolution.X, mousePos.Y / Graphics.ViewportResolution.Y);
        GBuffer? gBuffer = Camera.LastRenderedCamera?.GBuffer;
        
        if (gBuffer == null)
            return;
        
        int instanceID = gBuffer.GetObjectIDAt(mouseUV);
        if (instanceID == 0)
        {
            SetTarget(null);
            return;
        }
        
        Entity? e = EngineObject.FindObjectByID<Entity>(instanceID);
        SetTarget(e);
    }


    public void SetTarget(Entity? entity)
    {
        _target = entity;
    }


    protected override void DrawContent()
    {
        if (_target == null || _target.IsDestroyed)
        {
            ImGui.Text("No entity selected.");
            return;
        }
        
        ImGui.Text($"Entity: {_target.Name}");
        ImGui.Separator();
        
        DrawEntityHierarchy(_target);
    }


    private static void DrawEntityHierarchy(Entity entity)
    {
        // Inline destroy button
        if (ImGui.Button("Destroy"))
        {
            entity.Destroy();
            return;
        }
        ImGui.SameLine();
        
        // Transform hierarchy
        if (entity.HasChildren)
        {
            if (!ImGui.TreeNode(entity.Name))
                return;

            foreach (Entity child in entity.Children)
                DrawEntityHierarchy(child);

            ImGui.TreePop();
        }
        else
        {
            ImGui.Text(entity.Name);
        }
    }
}
#endif
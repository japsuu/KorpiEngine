using ImGuiNET;
using KorpiEngine.Core.EntityModel;

namespace KorpiEngine.Core.UI.DearImGui;

public class EntityEditor : ImGuiWindow
{
    private readonly Entity _target;

    public override string Title => $"Entity Editor - {_target.Name}";


    public EntityEditor(Entity target) : base(false)
    {
        _target = target;
    }


    protected override void DrawContent()
    {
        ImGui.Text($"Entity: {_target.Name}");
        ImGui.Separator();
        DrawEntityHierarchy(_target);
    }


    private static void DrawEntityHierarchy(Entity entity)
    {
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
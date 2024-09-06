#if TOOLS
using ImGuiNET;
using KorpiEngine.Entities;

namespace KorpiEngine.UI.DearImGui;

public abstract class EntityComponentEditor : ImGuiWindow
{
    public override string Title => $"Entity Component Editor - {_componentType}";
    protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

    private readonly EntityComponent _target;
    private readonly string _componentType;

    
    protected EntityComponentEditor(EntityComponent target) : base(false)
    {
        _target = target;
        _target.Destroying += Destroy;
        _componentType = _target.GetType().Name;
    }
    

    protected sealed override void DrawContent()
    {
        ImGui.Text($"Component: {_componentType}");
        ImGui.Text($"Entity: {_target.Entity.Name}");
        ImGui.Separator();
        
        DrawEditor();
    }
    
    
    protected abstract void DrawEditor();


    protected override void OnDestroy()
    {
        _target.Destroying -= Destroy;
    }
}
#endif
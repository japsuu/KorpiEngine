using ImGuiNET;
using KorpiEngine;
using KorpiEngine.SceneManagement;
using KorpiEngine.UI.DearImGui;
using Sandbox.Scenes.FullExample;
using Sandbox.Scenes.PrimitiveExample;
using Sandbox.Scenes.SponzaExample;

namespace Sandbox.Scenes;

public abstract class ExampleScene : Scene
{
    protected abstract string HelpTitle { get; }
    protected abstract string HelpText { get; }
    
    private HelpWindow _helpWindow = null!;
    
    
    protected override void OnLoad()
    {
        _helpWindow = new HelpWindow(HelpTitle, HelpText);
    }


    protected override void OnUnload()
    {
        _helpWindow.Destroy();
    }
}


public class HelpWindow(string title, string text) : ImGuiWindow(true)
{
    public override string Title => "Help";
    protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;


    protected sealed override void DrawContent()
    {
        ImGui.Text(title);
        ImGui.Separator();
        ImGui.TextWrapped(text);
        
        ImGui.Separator();

        if (ImGui.Button("Sponza Example Scene"))
            Application.SceneManager.LoadScene<SponzaExampleScene>(SceneLoadMode.Single);
        
        if (ImGui.Button("Full Example Scene"))
            Application.SceneManager.LoadScene<FullExampleScene>(SceneLoadMode.Single);
        
        if (ImGui.Button("Primitive Example Scene"))
            Application.SceneManager.LoadScene<PrimitiveExampleScene>(SceneLoadMode.Single);
    }
}
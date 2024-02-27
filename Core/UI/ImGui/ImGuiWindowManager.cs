using ImGuiNET;

namespace KorpiEngine.Core.UI.ImGui;

public static class ImGuiWindowManager
{
    private static readonly Dictionary<ImGuiWindow, string> RegisteredWindows = new();
    
    private static bool shouldRenderWindows = true;


    public static void RegisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Add(window, window.GetType().Name);
    }


    public static void UnregisterWindow(ImGuiWindow window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        RegisteredWindows.Remove(window);
    }


    internal static void Update()
    {
        ImGuiNET.ImGui.Begin("Windows", ImGuiWindowFlags.AlwaysAutoResize);
        ImGuiNET.ImGui.Checkbox("Draw Windows", ref shouldRenderWindows);
        ImGuiNET.ImGui.Separator();
        foreach (KeyValuePair<ImGuiWindow, string> kvp in RegisteredWindows)
        {
            bool windowVisible = kvp.Key.IsVisible;
            if (ImGuiNET.ImGui.Checkbox($"{kvp.Value} -> {kvp.Key.Title}", ref windowVisible))
                kvp.Key.ToggleVisibility();
        }
        ImGuiNET.ImGui.End();
        
        if (!shouldRenderWindows)
            return;
        
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Update();
    }


    internal static void Dispose()
    {
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Dispose();
    }
}
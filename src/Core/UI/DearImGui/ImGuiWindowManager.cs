﻿using ImGuiNET;

namespace KorpiEngine.UI.DearImGui;

public static class ImGuiWindowManager
{
    private static readonly Dictionary<ImGuiWindow, string> RegisteredWindows = new();
    
    private static bool shouldRenderWindows = true;


    public static void RegisterWindow(ImGuiWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

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
        ImGui.Begin("Windows", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Checkbox("Draw Windows", ref shouldRenderWindows);
        ImGui.Separator();
        foreach (KeyValuePair<ImGuiWindow, string> kvp in RegisteredWindows)
        {
            bool windowVisible = kvp.Key.IsVisible;
            if (ImGui.Checkbox($"{kvp.Value} -> {kvp.Key.Title}", ref windowVisible))
                kvp.Key.ToggleVisibility();
        }
        ImGui.End();
        
        if (!shouldRenderWindows)
            return;
        
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Update();
    }


    internal static void Shutdown()
    {
        foreach (ImGuiWindow window in RegisteredWindows.Keys)
            window.Destroy();
    }
}
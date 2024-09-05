using ImGuiNET;
using KorpiEngine.UI.DearImGui;

namespace KorpiEngine.UI;

/// <summary>
/// Collection of methods for drawing GUI elements from inside the OnDrawGUI method.
/// All GUI methods must be called between GUI.Begin and GUI.End.
/// </summary>
public static class GUI
{
    internal static bool AllowDraw { get; set; }
    internal static bool IsDrawing { get; private set; }
    
    public static bool WantCaptureKeyboard { get; internal set; }
    public static bool WantCaptureMouse { get; internal set; }
    public static DebugStatsWindow DebugStatsWindow { get; private set; } = null!;
    
    private static bool CanDraw => AllowDraw && IsDrawing;
    
    
    public static void Initialize()
    {
        DebugStatsWindow = new DebugStatsWindow();
    }
    
    
    public static void Deinitialize()
    {
        DebugStatsWindow.Destroy();
    }
    
    
    public static void Begin(string title, ImGuiWindowFlags flags = ImGuiWindowFlags.AlwaysAutoResize)
    {
        if (IsDrawing)
            return;

        ImGui.Begin(title, flags);
        
        IsDrawing = true;
    }
    
    
    public static void End()
    {
        if (!IsDrawing)
            return;
        
        ImGui.End();
        
        IsDrawing = false;
    }


    public static void Text(string text)
    {
        if (!CanDraw)
            return;
        
        ImGui.Text(text);
    }


    public static void FloatSlider(string text, ref float current, float min, float max)
    {
        if (!CanDraw)
            return;
        
        ImGui.SliderFloat(text, ref current, min, max);
    }
}
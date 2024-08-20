using ImGuiNET;

namespace KorpiEngine.Core.UI;

/// <summary>
/// Collection of methods for drawing GUI elements from inside the OnDrawGUI method.
/// All GUI methods must be called between GUI.Begin and GUI.End.
/// </summary>
public static class GUI
{
    internal static bool AllowDraw { get; set; }
    internal static bool IsDrawing { get; private set; }
    
    private static bool CanDraw => AllowDraw && IsDrawing;
    
    
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
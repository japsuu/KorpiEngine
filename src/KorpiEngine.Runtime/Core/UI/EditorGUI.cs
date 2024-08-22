#if TOOLS
using KorpiEngine.Core.UI.DearImGui;

namespace KorpiEngine.Core.UI;

public static class EditorGUI
{
    public static DebugStatsWindow DebugStatsWindow { get; private set; } = null!;
    public static EntityEditor EntityEditorWindow { get; private set; } = null!;
    
    
    public static void Initialize()
    {
        DebugStatsWindow = new DebugStatsWindow();
        EntityEditorWindow = new EntityEditor();
    }
    
    
    public static void Deinitialize()
    {
        DebugStatsWindow.Destroy();
        EntityEditorWindow.Destroy();
    }
}
#endif
using KorpiEngine.Core.UI.DearImGui;

namespace KorpiEngine.Core.UI;

#if DEBUG
public static class EditorGUI
{
    private static DebugStatsWindow debugStatsWindow = null!;
    private static EntityEditor entityEditorWindow = null!;
    
    
    public static void Initialize()
    {
        debugStatsWindow = new DebugStatsWindow();
        entityEditorWindow = new EntityEditor();
    }
    
    
    public static void Deinitialize()
    {
        debugStatsWindow.Destroy();
        entityEditorWindow.Destroy();
    }
}
#endif
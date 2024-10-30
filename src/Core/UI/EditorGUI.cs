#if KORPI_TOOLS
using KorpiEngine.UI.DearImGui;

namespace KorpiEngine.UI;

public static class EditorGUI
{
    public static EntityEditor EntityEditorWindow { get; private set; } = null!;
    
    
    public static void Initialize()
    {
        EntityEditorWindow = new EntityEditor();
    }
    
    
    public static void Deinitialize()
    {
        EntityEditorWindow.Destroy();
    }
}
#endif
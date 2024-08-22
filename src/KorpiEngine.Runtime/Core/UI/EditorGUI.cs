#if TOOLS
using KorpiEngine.Core.UI.DearImGui;

namespace KorpiEngine.Core.UI;

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
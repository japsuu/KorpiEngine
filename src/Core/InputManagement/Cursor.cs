using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.InputManagement;

public static class Cursor
{
    public static CursorLockState LockState
    {
        get => lockState;
        set
        {
            Debug.Assert(window != null, nameof(window) + " != null");
            if (LockState == value)
                return;
        
            window.CursorState = (CursorState)value;
            lockState = value;
        }
    }

    public static bool IsHidden => LockState != CursorLockState.None;
    
    private static GameWindow? window;
    private static CursorLockState lockState;
    
    
    internal static void Initialize(GameWindow gameWindow)
    {
        Debug.Assert(window == null, nameof(window) + " == null");
        window = gameWindow;
        lockState = CursorLockState.None;
    }
}
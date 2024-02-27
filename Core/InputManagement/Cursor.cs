using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.InputManagement;

public static class Cursor
{
    public static bool IsGrabbed { get; private set; }
    
    private static GameWindow? window;
    
    
    public static void ChangeGrabState()
    {
        Debug.Assert(window != null, nameof(window) + " != null");
        SetGrabbed(!IsGrabbed);
    }


    public static void SetGrabbed(bool shouldGrab)
    {
        Debug.Assert(window != null, nameof(window) + " != null");
        if (IsGrabbed == shouldGrab)
            return;
        
        if (shouldGrab)
        {
            window.CursorState = CursorState.Grabbed;
            IsGrabbed = true;
        }
        else
        {
            window.CursorState = CursorState.Normal;
            IsGrabbed = false;
        }
    }
    
    
    public static void Initialize(GameWindow gameWindow)
    {
        Debug.Assert(window == null, nameof(window) + " == null");
        window = gameWindow;
    }
}
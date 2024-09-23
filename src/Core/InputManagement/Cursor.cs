namespace KorpiEngine.InputManagement;

public static class Cursor
{
    private static CursorLockState lockState = CursorLockState.None;
    
    public static CursorLockState LockState
    {
        get => lockState;
        set
        {
            if (LockState == value)
                return;
        
            Application.SetCursorState(value);
            lockState = value;
        }
    }

    public static bool IsHidden => LockState != CursorLockState.None;
    
    
    internal static void Update(CursorLockState state)
    {
        LockState = state;
    }
}
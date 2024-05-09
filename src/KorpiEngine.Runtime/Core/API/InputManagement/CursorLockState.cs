namespace KorpiEngine.Core.API.InputManagement;

public enum CursorLockState
{
    /// The cursor is visible and the cursor motion is not limited.
    None,
    
    /// Hides the cursor.
    Hidden,
    
    /// Hides the cursor and locks it to the window.
    Locked,
}
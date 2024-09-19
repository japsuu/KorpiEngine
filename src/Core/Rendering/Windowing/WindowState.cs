namespace KorpiEngine.Rendering;

/// <summary>
/// Represents the display state of a window.
/// </summary>
public enum WindowState
{
    /// <summary>
    /// The window is in a normal (windowed) state.
    /// </summary>
    Normal,
    
    /// <summary>
    /// The window is minimized (iconified).
    /// </summary>
    Minimized,
    
    /// <summary>
    /// The window covers the whole working area, which includes the desktop but not the taskbar and/or panels.
    /// </summary>
    Maximized,
    
    /// <summary>
    /// The window covers the whole screen, including any taskbars and/or panels.
    /// </summary>
    Fullscreen
}
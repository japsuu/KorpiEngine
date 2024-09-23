using KorpiEngine.Mathematics;

namespace KorpiEngine.Rendering;

/// <summary>
/// Defines the settings for the window.
/// </summary>
public readonly struct WindowingSettings
{
    /// <summary>
    /// The text shown in the window title bar.
    /// </summary>
    public readonly string WindowTitle;
    
    /// <summary>
    /// The size of the window.
    /// <see cref="State"/> must not be set to <c>State.Fullscreen</c> for this to take effect.
    /// </summary>
    public readonly Int2 WindowSize;
    
    /// <summary>
    /// The display state of the window.
    /// </summary>
    public readonly WindowState State;


    public WindowingSettings(string windowTitle, Int2 windowSize, WindowState state)
    {
        WindowTitle = windowTitle;
        WindowSize = windowSize;
        State = state;
    }
    
    
    public static WindowingSettings Windowed(string windowTitle, Int2 windowSize)
    {
        return new WindowingSettings(windowTitle, windowSize, WindowState.Normal);
    }
    
    
    public static WindowingSettings Fullscreen(string windowTitle)
    {
        return new WindowingSettings(windowTitle, new Int2(1920, 1080), WindowState.Fullscreen);
    }
}
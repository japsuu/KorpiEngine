using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;

namespace KorpiEngine.Rendering;

public abstract class GraphicsWindow
{
    /// <summary>
    /// Event that is triggered when the window is resized.
    /// </summary>
    public abstract event Action<Int2>? OnResize;
    
    /// <summary>
    /// Gets the size of the window in screen coordinates (logical pixels).
    /// This is the size of the drawable area of the window, excluding any window decorations like borders and title bars.
    /// </summary>
    /// <example>
    /// Use <see cref="WindowSize"/> when you need to set or get the size of the window in logical pixels.
    /// For example, when centering the window on the screen:
    /// <code>
    /// var windowSize = graphicsWindow.WindowSize;
    /// var screenSize = DisplayInfo.ScreenSize;
    /// var centeredPosition = new Int2(
    ///     (screenSize.X - windowSize.X) / 2,
    ///     (screenSize.Y - windowSize.Y) / 2
    /// );
    /// graphicsWindow.SetPosition(centeredPosition);
    /// </code>
    /// </example>
    public abstract Int2 WindowSize { get; }
    
    /// <summary>
    /// Gets the size of the framebuffer in actual pixels.
    /// This is the size of the window in physical pixels, which can be larger than the window size on high-DPI displays.
    /// </summary>
    /// <example>
    /// Use <see cref="FramebufferSize"/> when you need to set or get the size of the framebuffer in physical pixels.
    /// For example, when updating the viewport for rendering:
    /// <code>
    /// var framebufferSize = graphicsWindow.FramebufferSize;
    /// graphics.UpdateViewport(framebufferSize.X, framebufferSize.Y);
    /// </code>
    /// </example>
    public abstract Int2 FramebufferSize { get; }
    
    /// <summary>
    /// Gets or sets if the window is visible on the screen.
    /// </summary>
    public abstract bool IsVisible { get; set; }
    
    /// <summary>
    /// Gets or sets how the window is displayed on the screen.
    /// </summary>
    public abstract WindowState WindowState { get; set; }
    
    /// <summary>
    /// Gets or sets how the cursor interacts with the window.
    /// </summary>
    public abstract CursorLockState CursorState { get; set; }


    /// <summary>
    /// Centers the window on the screen.
    /// </summary>
    public abstract void SetCentered();
    
    /// <summary>
    /// Sets the size of the window in screen coordinates (logical pixels).
    /// See <see cref="WindowSize"/> for more information.
    /// </summary>
    public abstract void SetSize(Int2 size);
    
    /// <summary>
    /// Sets the text displayed in the title bar of the window.
    /// </summary>
    public abstract void SetTitle(string title);
}
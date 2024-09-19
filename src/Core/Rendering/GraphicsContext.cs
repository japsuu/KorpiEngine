using KorpiEngine.InputManagement;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// Represents a graphics context that can be used to render graphics.
/// Handles the windowing system, input handling, and rendering.
/// </summary>
public abstract class GraphicsContext
{
    /// <summary>
    /// The window of the graphics context.
    /// Handles everything related to the windowing system.
    /// </summary>
    public abstract GraphicsWindow Window { get; }
    
    /// <summary>
    /// The graphics device of the graphics context.
    /// Handles everything related to rendering graphics.
    /// </summary>
    public abstract GraphicsDevice Device { get; }
    
    /// <summary>
    /// The current input state.
    /// Handles everything related to input handling and HID devices.
    /// </summary>
    public abstract InputState InputState { get; }
    
    /// <summary>
    /// The current state of the display.
    /// </summary>
    public abstract DisplayState DisplayState { get; }


    /// <summary>
    /// Initializes the graphics context, and enters the blocking run loop.
    /// </summary>
    /// <param name="windowingSettings">The settings for the window.</param>
    /// <param name="onLoad">The action to execute when the graphics context is loaded.</param>
    /// <param name="onUpdate">The action to execute when the graphics context is updated.</param>
    /// <param name="onRender">The action to execute when the graphics context is rendered.</param>
    /// <param name="onUnload">The action to execute when the graphics context is unloaded.</param>
    public abstract void Run(WindowingSettings windowingSettings, Action onLoad, Action<double> onUpdate, Action onRender, Action onUnload);
    
    
    /// <summary>
    /// Shuts down the graphics context.
    /// </summary>
    public abstract void Shutdown();
}
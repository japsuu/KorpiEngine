using KorpiEngine.Mathematics;

namespace KorpiEngine.InputManagement;

public abstract class InputState
{
    public abstract KeyboardState KeyboardState { get; }
    public abstract MouseState MouseState { get; }
}

public abstract class KeyboardState
{
    public abstract bool IsKeyDown(KeyCode key);
    public abstract bool IsKeyPressed(KeyCode key);
    public abstract bool IsKeyReleased(KeyCode key);
}

public abstract class MouseState
{
    /// <summary>
    /// The current mouse position.
    /// </summary>
    public abstract Vector2 Position { get; }
    
    /// <summary>
    /// The previous frame mouse position.
    /// </summary>
    public abstract Vector2 PreviousPosition { get; }
    
    /// <summary>
    /// The change in mouse position since the last frame.
    /// </summary>
    public abstract Vector2 PositionDelta { get; }
    
    /// <summary>
    /// The current scroll-wheel position.
    /// </summary>
    public abstract Vector2 Scroll { get; }
    
    /// <summary>
    /// The previous frame scroll-wheel position.
    /// </summary>
    public abstract Vector2 PreviousScroll { get; }
    
    /// <summary>
    /// The change in scroll-wheel position since the last frame.
    /// </summary>
    public abstract Vector2 ScrollDelta { get; }
    
    public abstract bool IsButtonDown(MouseButton button);
    public abstract bool IsButtonPressed(MouseButton button);
    public abstract bool IsButtonReleased(MouseButton button);
}
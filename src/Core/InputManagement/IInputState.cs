using KorpiEngine.Mathematics;

namespace KorpiEngine.InputManagement;

public interface IInputState
{
    public IKeyboardState KeyboardState { get; }
    public IMouseState MouseState { get; }
}

public interface IKeyboardState
{
    public bool IsKeyDown(KeyCode key);
    public bool IsKeyPressed(KeyCode key);
    public bool IsKeyReleased(KeyCode key);
}

public interface IMouseState
{
    /// <summary>
    /// The current mouse position.
    /// </summary>
    public Vector2 Position { get; }
    
    /// <summary>
    /// The previous frame mouse position.
    /// </summary>
    public Vector2 PreviousPosition { get; }
    
    /// <summary>
    /// The change in mouse position since the last frame.
    /// </summary>
    public Vector2 PositionDelta { get; }
    
    /// <summary>
    /// The current scroll-wheel position.
    /// </summary>
    public Vector2 Scroll { get; }
    
    /// <summary>
    /// The previous frame scroll-wheel position.
    /// </summary>
    public Vector2 PreviousScroll { get; }
    
    /// <summary>
    /// The change in scroll-wheel position since the last frame.
    /// </summary>
    public Vector2 ScrollDelta { get; }
    
    public bool IsButtonDown(MouseButton button);
    public bool IsButtonPressed(MouseButton button);
    public bool IsButtonReleased(MouseButton button);
}
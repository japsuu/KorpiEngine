using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButton = KorpiEngine.InputManagement.MouseButton;

namespace KorpiEngine.OpenGL;

internal class GLInputState(GameWindow window) : IInputState
{
    private readonly GLKeyboardState _keyboardState = new(window.KeyboardState);
    private readonly GLMouseState _mouseState = new(window.MouseState);
    
    public IKeyboardState KeyboardState => _keyboardState;
    public IMouseState MouseState => _mouseState;
}

internal class GLKeyboardState(KeyboardState keyboardState) : IKeyboardState
{
    public bool IsKeyDown(KeyCode key) => keyboardState.IsKeyDown((Keys)key);
    public bool IsKeyPressed(KeyCode key) => keyboardState.IsKeyPressed((Keys)key);
    public bool IsKeyReleased(KeyCode key) => keyboardState.IsKeyReleased((Keys)key);
}

internal class GLMouseState(MouseState mouseState) : IMouseState
{
    public Vector2 Position => new(mouseState.X, mouseState.Y);
    public Vector2 PreviousPosition => new(mouseState.PreviousX, mouseState.PreviousY);
    public Vector2 PositionDelta => new(mouseState.Delta.X, mouseState.Delta.Y);
    
    public Vector2 Scroll => new(mouseState.Scroll.X, mouseState.Scroll.Y);
    public Vector2 PreviousScroll => new(mouseState.PreviousScroll.X, mouseState.PreviousScroll.Y);
    public Vector2 ScrollDelta => new(mouseState.ScrollDelta.X, mouseState.ScrollDelta.Y);

    public bool IsButtonDown(MouseButton button) => mouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public bool IsButtonPressed(MouseButton button) => mouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public bool IsButtonReleased(MouseButton button) => mouseState.IsButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
}
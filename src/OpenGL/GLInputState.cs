using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KeyboardState = KorpiEngine.InputManagement.KeyboardState;
using MouseButton = KorpiEngine.InputManagement.MouseButton;
using MouseState = KorpiEngine.InputManagement.MouseState;

namespace KorpiEngine.OpenGL;

internal class GLInputState : InputState
{
    private readonly GLKeyboardState _keyboardState;
    private readonly GLMouseState _mouseState;
    
    public override KeyboardState KeyboardState => _keyboardState;
    public override MouseState MouseState => _mouseState;


    public GLInputState(GameWindow window)
    {
        _mouseState = new GLMouseState();
        _keyboardState = new GLKeyboardState();
        
        Update(window);
    }


    public void Update(GameWindow window)
    {
        _keyboardState.Update(window.KeyboardState);
        _mouseState.Update(window.MouseState);
    }
}

internal class GLKeyboardState : KeyboardState
{
    private OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState _keyboardState = null!;


    public void Update(OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState keyboardState)
    {
        _keyboardState = keyboardState;
    }
    
    
    public override bool IsKeyDown(KeyCode key) => _keyboardState.IsKeyDown((Keys)key);
    public override bool IsKeyPressed(KeyCode key) => _keyboardState.IsKeyPressed((Keys)key);
    public override bool IsKeyReleased(KeyCode key) => _keyboardState.IsKeyReleased((Keys)key);
}

internal class GLMouseState : MouseState
{
    private OpenTK.Windowing.GraphicsLibraryFramework.MouseState _mouseState = null!;
    
    public override Vector2 Position => new(_mouseState.X, _mouseState.Y);
    public override Vector2 PreviousPosition => new(_mouseState.PreviousX, _mouseState.PreviousY);
    public override Vector2 PositionDelta => new(_mouseState.Delta.X, _mouseState.Delta.Y);
    
    public override Vector2 Scroll => new(_mouseState.Scroll.X, _mouseState.Scroll.Y);
    public override Vector2 PreviousScroll => new(_mouseState.PreviousScroll.X, _mouseState.PreviousScroll.Y);
    public override Vector2 ScrollDelta => new(_mouseState.ScrollDelta.X, _mouseState.ScrollDelta.Y);

    public override bool IsButtonDown(MouseButton button) => _mouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public override bool IsButtonPressed(MouseButton button) => _mouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public override bool IsButtonReleased(MouseButton button) => _mouseState.IsButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);


    public void Update(OpenTK.Windowing.GraphicsLibraryFramework.MouseState mouseState)
    {
        _mouseState = mouseState;
    }
}
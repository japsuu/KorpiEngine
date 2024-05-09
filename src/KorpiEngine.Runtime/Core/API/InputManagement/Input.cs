using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KorpiEngine.Core.API.InputManagement;

public static class Input
{
    internal static KeyboardState KeyboardState = null!;
    internal static MouseState MouseState = null!;
    
    public static Vector2 MousePosition => new(MouseState.X, MouseState.Y);
    public static Vector2 MouseDelta => new(MouseState.Delta.X, MouseState.Delta.Y);
    public static Vector2 ScrollDelta => new(MouseState.Scroll.X, MouseState.Scroll.Y);
    public static float MouseX => MouseState.X;
    public static float MouseY => MouseState.Y;
    public static float MousePreviousX => MouseState.PreviousX;
    public static float MousePreviousY => MouseState.PreviousY;


    public static void Update(KeyboardState kState, MouseState mState)
    {
        KeyboardState = kState;
        MouseState = mState;
    }


    public static bool GetKey(KeyCode key) => KeyboardState.IsKeyDown((Keys)key);
    public static bool GetKeyDown(KeyCode key) => KeyboardState.IsKeyPressed((Keys)key);
    public static bool GetKeyUp(KeyCode key) => KeyboardState.IsKeyReleased((Keys)key);
    
    public static bool GetMouse(MouseButton button) => MouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public static bool GetMouseDown(MouseButton button) => MouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
    public static bool GetMouseUp(MouseButton button) => MouseState.IsButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
}
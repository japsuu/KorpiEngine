using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace KorpiEngine.InputManagement;

public static class Input
{
    internal static KeyboardState KeyboardState { get; private set; } = null!;
    internal static MouseState MouseState { get; private set; } = null!;
    
    public static Vector2 MousePosition => new(MouseState.Position.X, Graphics.ViewportResolution.Y - MouseState.Position.Y);
    public static float MouseX => MouseState.Position.X;
    public static float MouseY => MouseState.Position.Y;
    
    public static Vector2 MousePreviousPosition => new(MouseState.PreviousPosition.X, Graphics.ViewportResolution.Y - MouseState.PreviousPosition.Y);
    public static float MousePreviousX => MouseState.PreviousPosition.X;
    public static float MousePreviousY => MouseState.PreviousPosition.Y;
    
    public static Vector2 MouseDelta => new(MouseState.PositionDelta.X, MouseState.PositionDelta.Y);
    public static Vector2 ScrollDelta => new(MouseState.ScrollDelta.X, MouseState.ScrollDelta.Y);


    public static void Update(InputState inputState)
    {
        KeyboardState = inputState.KeyboardState;
        MouseState = inputState.MouseState;
    }


    public static bool GetKey(KeyCode key) => KeyboardState.IsKeyDown(key);
    public static bool GetKeyDown(KeyCode key) => KeyboardState.IsKeyPressed(key);
    public static bool GetKeyUp(KeyCode key) => KeyboardState.IsKeyReleased(key);
    
    public static bool GetMouseButton(MouseButton button) => MouseState.IsButtonDown(button);
    public static bool GetMouseButtonDown(MouseButton button) => MouseState.IsButtonPressed(button);
    public static bool GetMouseButtonUp(MouseButton button) => MouseState.IsButtonReleased(button);
}
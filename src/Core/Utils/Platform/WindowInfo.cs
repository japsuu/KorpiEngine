using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace KorpiEngine.Utils;

public static class WindowInfo
{
    public readonly struct WindowResizeEventArgs(int width, int height, float aspectRatio)
    {
        public readonly int Width = width;
        public readonly int Height = height;
        public readonly float AspectRatio = aspectRatio;
    }
    
    /// <summary>
    /// Called when the client window has resized.
    /// </summary>
    public static event Action<WindowResizeEventArgs>? WindowResized;
    
    /// <summary>
    /// Size of the client window.
    /// </summary>
    public static Int2 ClientSize { get; private set; }
    
    /// <summary>
    /// Width of the client window.
    /// </summary>
    public static int ClientWidth => ClientSize.X;
    
    /// <summary>
    /// Height of the client window.
    /// </summary>
    public static int ClientHeight => ClientSize.Y;
    
    /// <summary>
    /// Aspect ratio of the client window.
    /// </summary>
    public static float ClientAspectRatio { get; private set; }


    internal static void Update(GraphicsContext context)
    {
        ClientSize = context.Window.WindowSize;
        ClientAspectRatio = ClientWidth / (float)ClientHeight;
        WindowResized?.Invoke(new WindowResizeEventArgs(ClientWidth, ClientHeight, ClientAspectRatio));
    }
}
using KorpiEngine.Mathematics;
using OpenTK.Windowing.Desktop;

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
    public static event Action<WindowResizeEventArgs>? ClientResized;
    
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
    
    
    public static void Initialize(NativeWindow window)
    {
        UpdateSize(window.ClientSize.X, window.ClientSize.Y);
        window.Resize += args => UpdateSize(args.Width, args.Height);
    }


    private static void UpdateSize(int width, int height)
    {
        ClientSize = new Int2(width, height);
        ClientAspectRatio = ClientWidth / (float)ClientHeight;
        ClientResized?.Invoke(new WindowResizeEventArgs(ClientWidth, ClientHeight, ClientAspectRatio));
    }
}
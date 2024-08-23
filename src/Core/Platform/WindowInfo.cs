using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.Platform;

public static class WindowInfo
{
    public readonly struct WindowResizeEventArgs
    {
        public readonly int Width;
        public readonly int Height;
        public readonly float AspectRatio;


        public WindowResizeEventArgs(int width, int height, float aspectRatio)
        {
            Width = width;
            Height = height;
            AspectRatio = aspectRatio;
        }
    }
    
    /// <summary>
    /// Called when the client window has resized.
    /// </summary>
    public static event Action<WindowResizeEventArgs>? ClientResized;
    
    /// <summary>
    /// Width of the client window.
    /// </summary>
    public static int ClientWidth { get; private set; }
    
    /// <summary>
    /// Height of the client window.
    /// </summary>
    public static int ClientHeight { get; private set; }
    
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
        ClientWidth = width;
        ClientHeight = height;
        ClientAspectRatio = ClientWidth / (float)ClientHeight;
        ClientResized?.Invoke(new WindowResizeEventArgs(ClientWidth, ClientHeight, ClientAspectRatio));
    }
}
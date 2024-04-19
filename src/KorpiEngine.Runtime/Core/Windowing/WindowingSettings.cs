using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.Windowing;

/// <summary>
/// Contains the initial windowing configuration for a <see cref="Game"/>.
/// </summary>
public struct WindowingSettings
{
    public GameWindowSettings GameWindowSettings { get; }
    public NativeWindowSettings NativeWindowSettings { get; }

    public WindowingSettings(Vector2i windowSize, string windowTitle)
    {
        GameWindowSettings gws = new()
        {
            UpdateFrequency = 0,
            Win32SuspendTimerOnDrag = false
        };
        
        NativeWindowSettings nws = new()
        {
            ClientSize = new Vector2i(windowSize.X, windowSize.Y),
            StartVisible = false,
            Title = $"{EngineConstants.ENGINE_NAME} {EngineConstants.ENGINE_VERSION} - {windowTitle}",
            NumberOfSamples = 0,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 2),
            AspectRatio = (16, 9),
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        };
        
        GameWindowSettings = gws;
        NativeWindowSettings = nws;
    }
}
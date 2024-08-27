using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Windowing;

/// <summary>
/// Contains the initial windowing configuration for a <see cref="Application"/>.
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
            ClientSize = new OpenTK.Mathematics.Vector2i(windowSize.X, windowSize.Y),
            StartVisible = false,
            Title = $"{EngineConstants.ENGINE_NAME} {EngineConstants.ENGINE_VERSION} - {windowTitle}",
            NumberOfSamples = 0,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 2),
            AspectRatio = (16, 9),
#if TOOLS
            Flags = ContextFlags.Debug
#endif
        };
        
        GameWindowSettings = gws;
        NativeWindowSettings = nws;
    }
}
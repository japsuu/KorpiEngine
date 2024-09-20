using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using WindowState = KorpiEngine.Rendering.WindowState;

namespace KorpiEngine.OpenGL;

public sealed class GLWindow : GraphicsWindow
{
    private readonly GameWindow _internalWindow;
    private readonly GLInputState _inputState;
    private readonly Action _onLoad;
    private readonly Action<double> _onUpdate;
    private readonly Action _onRender;
    private readonly Action _onUnload;

    public override event Action<Int2>? OnResize;

    public override string Title
    {
        get => _internalWindow.Title;
        set => _internalWindow.Title = value;
    }

    public override Int2 WindowSize
    {
        get => new(_internalWindow.ClientSize.X, _internalWindow.ClientSize.Y);
        set => _internalWindow.ClientSize = new Vector2i(value.X, value.Y);
    }

    public override Int2 FramebufferSize => new(_internalWindow.FramebufferSize.X, _internalWindow.FramebufferSize.Y);

    public override bool IsVisible
    {
        get => _internalWindow.IsVisible;
        set => _internalWindow.IsVisible = value;
    }
    
    public override WindowState WindowState
    {
        get
        {
            return _internalWindow.WindowState switch
            {
                OpenTK.Windowing.Common.WindowState.Normal => WindowState.Normal,
                OpenTK.Windowing.Common.WindowState.Minimized => WindowState.Minimized,
                OpenTK.Windowing.Common.WindowState.Maximized => WindowState.Maximized,
                OpenTK.Windowing.Common.WindowState.Fullscreen => WindowState.Fullscreen,
                _ => WindowState.Normal
            };
        }
        set
        {
            _internalWindow.WindowState = value switch
            {
                WindowState.Normal => OpenTK.Windowing.Common.WindowState.Normal,
                WindowState.Minimized => OpenTK.Windowing.Common.WindowState.Minimized,
                WindowState.Maximized => OpenTK.Windowing.Common.WindowState.Maximized,
                WindowState.Fullscreen => OpenTK.Windowing.Common.WindowState.Fullscreen,
                _ => OpenTK.Windowing.Common.WindowState.Normal
            };
        }
        
    }

    public override CursorLockState CursorState 
    {
        get
        {
            return _internalWindow.CursorState switch
            {
                OpenTK.Windowing.Common.CursorState.Normal => CursorLockState.None,
                OpenTK.Windowing.Common.CursorState.Hidden => CursorLockState.Hidden,
                OpenTK.Windowing.Common.CursorState.Grabbed => CursorLockState.Locked,
                _ => CursorLockState.None
            };
        }
        set
        {
            _internalWindow.CursorState = value switch
            {
                CursorLockState.None => OpenTK.Windowing.Common.CursorState.Normal,
                CursorLockState.Hidden => OpenTK.Windowing.Common.CursorState.Hidden,
                CursorLockState.Locked => OpenTK.Windowing.Common.CursorState.Grabbed,
                _ => OpenTK.Windowing.Common.CursorState.Normal
            };
        }
    }

    public InputState InputState => _inputState;


    public DisplayState DisplayState
    {
        get
        {
            MonitorInfo monitor = Monitors.GetMonitorFromWindow(_internalWindow);
            return new DisplayState(new Int2(monitor.HorizontalResolution, monitor.VerticalResolution));
        }
    }


    public GLWindow(WindowingSettings windowingSettings, Action onLoad, Action<double> onUpdate, Action onRender, Action onUnload)
    {
        (GameWindowSettings gws, NativeWindowSettings nws) = GetWindowSettings(windowingSettings);

        _onLoad = onLoad;
        _onUpdate = onUpdate;
        _onRender = onRender;
        _onUnload = onUnload;
        
        _internalWindow = new GameWindow(gws, nws);
        
        _internalWindow.Load += OnLoad;
        _internalWindow.UpdateFrame += OnUpdate;
        _internalWindow.RenderFrame += OnRender;
        _internalWindow.Unload += OnUnload;
        _internalWindow.Resize += OnWindowResize;
        
        _inputState = new GLInputState(_internalWindow);
    }


    public void Run()
    {
        _internalWindow.Run();
    }
    

    public override void SetCentered()
    {
        _internalWindow.CenterWindow();
    }


    public void Shutdown()
    {
        _internalWindow.Close();
        _internalWindow.Dispose();
    }


    private void OnLoad()
    {
        _onLoad();
    }


    private void OnUnload()
    {
        _onUnload();
    }
    
    
    private void OnUpdate(FrameEventArgs args)
    {
        _onUpdate(args.Time);
    }


    private void OnRender(FrameEventArgs args)
    {
        _onRender();
        
        _internalWindow.SwapBuffers();
    }


    private void OnWindowResize(ResizeEventArgs e)
    {
        OnResize?.Invoke(new Int2(e.Width, e.Height));
    }


    private static (GameWindowSettings gws, NativeWindowSettings nws) GetWindowSettings(WindowingSettings windowingSettings)
    {
        WindowState state = windowingSettings.State;
        Int2 windowSize = windowingSettings.WindowSize;
        string windowTitle = windowingSettings.WindowTitle;
        
        GameWindowSettings gws = new()
        {
            UpdateFrequency = 0,
            Win32SuspendTimerOnDrag = false
        };
        
        NativeWindowSettings nws = new()
        {
            WindowState = state switch
            {
                WindowState.Normal => OpenTK. Windowing. Common. WindowState.Normal,
                WindowState.Minimized => OpenTK. Windowing. Common. WindowState.Minimized,
                WindowState.Maximized => OpenTK. Windowing. Common. WindowState.Maximized,
                WindowState.Fullscreen => OpenTK. Windowing. Common. WindowState.Fullscreen,
                _ => OpenTK. Windowing. Common. WindowState.Normal
            },
            ClientSize = new Vector2i(windowSize.X, windowSize.Y),
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
        return (gws, nws);
    }
}
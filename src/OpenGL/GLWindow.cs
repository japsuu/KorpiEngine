using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Vector2 = KorpiEngine.Mathematics.Vector2;
using WindowState = KorpiEngine.Rendering.WindowState;

namespace KorpiEngine.OpenGL;

public sealed class GLWindow : GraphicsWindow
{
    private readonly GameWindow _internalWindow;
    private readonly GLInputState _inputState;
    private readonly Action _onLoad;
    private readonly Action _onFrameStart;
    private readonly Action<double> _onFrameUpdate;
    private readonly Action _onFrameRender;
    private readonly Action _onFrameEnd;
    private readonly Action _onUnload;
    
    private MonitorInfo _currentMonitor;

    public override event Action<Int2>? OnResize;
    public override event Action<char>? OnTextInput;
    public override event Action<Vector2>? OnMouseWheel;

    public override string Title
    {
        get => _internalWindow.Title;
        set => _internalWindow.Title = value;
    }

    public override Int2 Size
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

    public IInputState InputState => _inputState;


    public DisplayState DisplayState => new(new Int2(_currentMonitor.HorizontalResolution, _currentMonitor.VerticalResolution));


    public GLWindow(WindowingSettings windowingSettings, Action onLoad, Action onFrameStart, Action<double> onFrameUpdate, Action onFrameRender, Action onFrameEnd, Action onUnload)
    {
        (GameWindowSettings gws, NativeWindowSettings nws) = GetWindowSettings(windowingSettings);

        _onLoad = onLoad;
        _onFrameStart = onFrameStart;
        _onFrameUpdate = onFrameUpdate;
        _onFrameRender = onFrameRender;
        _onFrameEnd = onFrameEnd;
        _onUnload = onUnload;
        
        _internalWindow = new GameWindow(gws, nws);
        
        _internalWindow.Load += OnWindowLoad;
        _internalWindow.UpdateFrame += OnWindowUpdate;
        _internalWindow.RenderFrame += OnWindowRender;
        _internalWindow.Unload += OnWindowUnload;
        _internalWindow.Resize += OnWindowResize;
        _internalWindow.TextInput += OnWindowTextInput;
        _internalWindow.MouseWheel += OnWindowMouseWheel;
        _internalWindow.Move += OnWindowMove;
        
        _currentMonitor = Monitors.GetMonitorFromWindow(_internalWindow);
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


    private void OnWindowLoad()
    {
        _onLoad();
    }


    private void OnWindowUnload()
    {
        _onUnload();
    }
    
    
    private void OnWindowUpdate(FrameEventArgs args)
    {
        _onFrameStart();
        _onFrameUpdate(args.Time);
    }


    private void OnWindowRender(FrameEventArgs args)
    {
        _onFrameRender();
        
        _internalWindow.SwapBuffers();
        _onFrameEnd();
    }


    private void OnWindowResize(ResizeEventArgs e)
    {
        OnResize?.Invoke(new Int2(e.Width, e.Height));
    }
    
    
    private void OnWindowTextInput(TextInputEventArgs e)
    {
        OnTextInput?.Invoke((char)e.Unicode);
    }
    
    
    private void OnWindowMouseWheel(MouseWheelEventArgs e)
    {
        OnMouseWheel?.Invoke(new Vector2(e.OffsetX, e.OffsetY));
    }


    private void OnWindowMove(WindowPositionEventArgs obj)
    {
        _currentMonitor = Monitors.GetMonitorFromWindow(_internalWindow);
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
#if KORPI_TOOLS
            Flags = ContextFlags.Debug
#endif
        };
        return (gws, nws);
    }
}
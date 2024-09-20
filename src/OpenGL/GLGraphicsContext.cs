using KorpiEngine.InputManagement;
using KorpiEngine.Rendering;
using KorpiEngine.Utils;

namespace KorpiEngine.OpenGL;

public class GLGraphicsContext : GraphicsContext
{
    private const string NOT_INITIALIZED = "Graphics context not initialized.";
    
    private GLGraphicsDevice? _device;
    private GLWindow? _window;


    public override GraphicsDevice Device => _device ?? throw new InvalidOperationException(NOT_INITIALIZED);
    public override GraphicsWindow Window => _window ?? throw new InvalidOperationException(NOT_INITIALIZED);
    
    public override InputState InputState => _window?.InputState ?? throw new InvalidOperationException(NOT_INITIALIZED);
    public override DisplayState DisplayState => _window?.DisplayState ?? throw new InvalidOperationException(NOT_INITIALIZED);

    
    public override void Run(WindowingSettings windowingSettings, Action onLoad, Action<double> onUpdate, Action onRender, Action onUnload)
    {
        _window = new GLWindow(windowingSettings, onLoad, onUpdate, onRender, onUnload);
        _device = new GLGraphicsDevice();

        _window.Run();
    }


    public override void Shutdown()
    {
        _device?.Shutdown();
        _window?.Shutdown();
        _device = null;
        _window = null;
    }
}
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.Rendering.GraphicsDrivers;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
public class GLGraphicsDriver : GraphicsDriver
{
    protected override void InitializeInternal() { }


    protected override void ShutdownInternal() { }
}

public abstract class GraphicsDriver
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsDriver));
    
    
    public void Initialize()
    {
        Logger.Info("Initializing...");
        InitializeInternal();
    }


    protected abstract void InitializeInternal();


    public void Shutdown()
    {
        Logger.Info("Shutting down...");
        ShutdownInternal();
    }


    protected abstract void ShutdownInternal();
}
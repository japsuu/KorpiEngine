using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.GraphicsDrivers;

public abstract class GraphicsDriver
{
    protected static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsDriver));


    #region SHUTDOWN AND INITIALIZATION

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

    #endregion
    
    public abstract void SetClearColor(float r, float g, float b, float a);
    
    public abstract void SetClearColor(Color color);

    public abstract void UpdateViewport(int x, int y, int width, int height);

    public abstract void Clear(ClearBufferMask mask);

    public abstract void Enable(EnableCap mask);
}
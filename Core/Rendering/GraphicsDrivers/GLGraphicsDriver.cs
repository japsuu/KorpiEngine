namespace KorpiEngine.Core.Rendering.GraphicsDrivers;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
public class GLGraphicsDriver : GraphicsDriver
{
    public override void Initialize() { }


    public override void Shutdown() { }
}

public abstract class GraphicsDriver
{
    public abstract void Initialize();
    
    public abstract void Shutdown();
}
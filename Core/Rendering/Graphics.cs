using KorpiEngine.Core.Rendering.GraphicsDrivers;

namespace KorpiEngine.Core.Rendering;

public static class Graphics
{
    public static GraphicsDriver Driver { get; private set; } = null!;


    public static void Initialize(GraphicsDriver driver)
    {
        Driver = driver;
        Driver.Initialize();
    }
    
    
    public static void Shutdown()
    {
        Driver.Shutdown();
    }
}
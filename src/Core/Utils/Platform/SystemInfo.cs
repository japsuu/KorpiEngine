using System.Runtime.InteropServices;
using KorpiEngine.Rendering;

namespace KorpiEngine.Utils;

/// <summary>
/// Contains information about the current system.
/// </summary>
public static class SystemInfo
{
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsFreeBSD() => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
    public static bool IsMac() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);


    /// <summary>
    /// Gets the number of processors available to the current process.
    /// </summary>
    public static int ProcessorCount { get; private set; }

    /// <summary>
    /// ID of the thread updating the main window.
    /// </summary>
    public static int MainThreadId { get; private set; }
    
    /// <summary>
    /// Maximum supported texture size of the current graphics driver.
    /// </summary>
    public static int MaxTextureSize { get; private set; }
    
    /// <summary>
    /// Maximum supported cube map texture size of the current graphics driver.
    /// </summary>
    public static int MaxCubeMapTextureSize { get; private set; }
    
    /// <summary>
    /// Maximum supported array texture layers of the current graphics driver.
    /// </summary>
    public static int MaxArrayTextureLayers { get; private set; }
    
    /// <summary>
    /// Maximum supported framebuffer color attachments of the current graphics driver.
    /// </summary>
    public static int MaxFramebufferColorAttachments { get; private set; }
    
    
    public static void Initialize(GraphicsContext context)
    {
        ProcessorCount = Environment.ProcessorCount;
        MainThreadId = Environment.CurrentManagedThreadId;

        MaxTextureSize = context.Device.MaxTextureSize;
        MaxCubeMapTextureSize = context.Device.MaxCubeMapTextureSize;
        MaxArrayTextureLayers = context.Device.MaxArrayTextureLayers;
        MaxFramebufferColorAttachments = context.Device.MaxFramebufferColorAttachments;
    }
}
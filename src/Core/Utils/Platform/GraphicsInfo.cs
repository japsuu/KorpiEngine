using KorpiEngine.Rendering;

namespace KorpiEngine.Utils;

/// <summary>
/// Contains information about the current graphics driver.
/// </summary>
public static class GraphicsInfo
{
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
        MaxTextureSize = context.Device.MaxTextureSize;
        MaxCubeMapTextureSize = context.Device.MaxCubeMapTextureSize;
        MaxArrayTextureLayers = context.Device.MaxArrayTextureLayers;
        MaxFramebufferColorAttachments = context.Device.MaxFramebufferColorAttachments;
    }
}
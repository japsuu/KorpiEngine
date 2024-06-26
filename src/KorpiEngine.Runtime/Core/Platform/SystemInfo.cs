﻿namespace KorpiEngine.Core.Platform;

/// <summary>
/// Contains information about the current system.
/// </summary>
public static class SystemInfo
{
    /// <summary>
    /// Gets the number of processors available to the current process.
    /// </summary>
    public static int ProcessorCount { get; internal set; }

    /// <summary>
    /// Id of the thread updating the main window.
    /// </summary>
    public static int MainThreadId { get; internal set; }
    
    /// <summary>
    /// Maximum supported texture size of the current graphics driver.
    /// </summary>
    public static int MaxTextureSize { get; internal set; }
    
    /// <summary>
    /// Maximum supported cube map texture size of the current graphics driver.
    /// </summary>
    public static int MaxCubeMapTextureSize { get; internal set; }
    
    /// <summary>
    /// Maximum supported array texture layers of the current graphics driver.
    /// </summary>
    public static int MaxArrayTextureLayers { get; internal set; }
    
    /// <summary>
    /// Maximum supported framebuffer color attachments of the current graphics driver.
    /// </summary>
    public static int MaxFramebufferColorAttachments { get; internal set; }
}
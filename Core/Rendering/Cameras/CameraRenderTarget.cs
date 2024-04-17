namespace KorpiEngine.Core.Rendering.Cameras;

/// <summary>
/// The render target for a camera.
/// </summary>
public enum CameraRenderTarget
{
    /// <summary>
    /// The camera renders to the screen.
    /// </summary>
    Screen,
    
    /// <summary>
    /// The camera renders to a texture.
    /// </summary>
    Texture
}
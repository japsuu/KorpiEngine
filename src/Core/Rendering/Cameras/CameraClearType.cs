namespace KorpiEngine.Rendering.Cameras;

/// <summary>
/// Defines how the camera clears the screen.
/// </summary>
public enum CameraClearType
{
    /// <summary>
    /// The camera does not clear the screen.
    /// </summary>
    Nothing,
    
    /// <summary>
    /// The camera clears the screen with a solid color.
    /// </summary>
    SolidColor,
}
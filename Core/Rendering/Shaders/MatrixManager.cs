using KorpiEngine.Core.Platform;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Shaders;

/// <summary>
/// Manages matrix data.
/// 
/// The manager keeps track of the projection and view matrices and provides the
/// screen position of a world position.
/// 
/// The manager also provides events for when the projection and view matrices are changed.
/// </summary>
public static class MatrixManager
{
    public static event Action<Matrix4>? ProjectionMatrixChanged;
    public static event Action<Matrix4>? ViewMatrixChanged;

    private static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
    private static Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;
    
    
    internal static void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        ProjectionMatrix = projectionMatrix;
        ProjectionMatrixChanged?.Invoke(projectionMatrix);
    }
    
    
    internal static void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        ViewMatrix = viewMatrix;
        ViewMatrixChanged?.Invoke(viewMatrix);
    }
    
    
    /// <returns>If the provided world position is visible on screen.</returns>
    public static bool WorldPositionToScreenPosition(Vector3 worldPosition, out Vector2 screenPos)
    {
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1) * ViewMatrix * ProjectionMatrix;
        
        // Without this the coordinates are visible even when looking straight away from them.
        if (clipSpacePosition.W <= 0)
        {
            screenPos = Vector2.NegativeInfinity;
            return false;
        }
        
        Vector3 normalizedDeviceCoordinates = clipSpacePosition.Xyz / clipSpacePosition.W;
        Vector2 screenCoordinates = new Vector2(normalizedDeviceCoordinates.X, -normalizedDeviceCoordinates.Y);
        screenCoordinates += Vector2.One;
        screenCoordinates /= 2;
        screenCoordinates.X *= WindowInfo.ClientWidth;
        screenCoordinates.Y *= WindowInfo.ClientHeight;
        screenPos = screenCoordinates;
        return true;
    }
}
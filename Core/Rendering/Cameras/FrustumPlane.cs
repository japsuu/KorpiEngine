using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Cameras;

public class FrustumPlane
{
    /// <summary>
    /// Normal unit vector.
    /// </summary>
    public Vector3 Normal;
    
    /// <summary>
    /// Distance from origin to the nearest point in the plane.
    /// </summary>
    public float Distance;
}
namespace KorpiEngine.Rendering.Cameras;

public struct FrustumPlane
{
    /// <summary>
    /// Normal unit vector.
    /// </summary>
    public Vector3 Normal { get; set; }
    
    /// <summary>
    /// Distance from origin to the nearest point in the plane.
    /// </summary>
    public double Distance { get; set; }
}
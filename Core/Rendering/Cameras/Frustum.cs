namespace KorpiEngine.Core.Rendering.Cameras;

public class Frustum
{
    public readonly FrustumPlane Top;
    public readonly FrustumPlane Bottom;
    public readonly FrustumPlane Right;
    public readonly FrustumPlane Left;
    public readonly FrustumPlane Far;
    public readonly FrustumPlane Near;
    public readonly FrustumPlane[] Planes;
    
    
    public Frustum()
    {
        Planes = new FrustumPlane[6];
        Planes[0] = Top = new FrustumPlane();
        Planes[1] = Bottom = new FrustumPlane();
        Planes[2] = Right = new FrustumPlane();
        Planes[3] = Left = new FrustumPlane();
        Planes[4] = Far = new FrustumPlane();
        Planes[5] = Near = new FrustumPlane();
    }
}
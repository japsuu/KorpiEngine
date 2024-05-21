namespace KorpiEngine.Core.Rendering.Cameras;

public struct Frustum
{
    public FrustumPlane Top;
    public FrustumPlane Bottom;
    public FrustumPlane Right;
    public FrustumPlane Left;
    public FrustumPlane Far;
    public FrustumPlane Near;
    public readonly FrustumPlane[] Planes;
    
    
    public Frustum(FrustumPlane[] planes)
    {
        Planes = planes;
        Top = Planes[0];
        Bottom = Planes[1];
        Right = Planes[2];
        Left = Planes[3];
        Far = Planes[4];
        Near = Planes[5];
    }
}
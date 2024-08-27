namespace KorpiEngine.Rendering.Cameras;

public struct Frustum(FrustumPlane[] planes)
{
    public readonly FrustumPlane Top = planes[0];
    public readonly FrustumPlane Bottom = planes[1];
    public readonly FrustumPlane Right = planes[2];
    public readonly FrustumPlane Left = planes[3];
    public readonly FrustumPlane Far = planes[4];
    public readonly FrustumPlane Near = planes[5];
    public readonly FrustumPlane[] Planes = planes;
}
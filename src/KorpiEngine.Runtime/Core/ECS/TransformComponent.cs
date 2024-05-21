using KorpiEngine.Core.API;

namespace KorpiEngine.Core.ECS;

public struct TransformComponent() : INativeComponent
{
    public Vector3 LocalPosition = Vector3.Zero;
    public Vector3 LocalScale = Vector3.One;
    public Quaternion LocalRotation = Quaternion.Identity;
    public uint Version = 1;
}
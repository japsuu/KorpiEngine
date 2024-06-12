using KorpiEngine.Core.API;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.ECS;

public struct TransformComponent() : INativeComponent
{
    public Transform? Parent = null;
    public List<Transform> Children = [];
    public Vector3 LocalPosition = Vector3.Zero;
    public Vector3 LocalScale = Vector3.One;
    public Quaternion LocalRotation = Quaternion.Identity;
    public uint Version = 1;
}
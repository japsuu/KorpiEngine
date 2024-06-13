using KorpiEngine.Core.API;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.ECS;

public struct TransformComponent() : INativeComponent
{
    public Transform? Parent = null;    //TODO. Change to EntityRef
    public List<Transform> Children = [];   //TODO. Change to EntityRef
    public Vector3 LocalPosition = Vector3.Zero;
    public Vector3 LocalScale = Vector3.One;
    public Quaternion LocalRotation = Quaternion.Identity;
    public uint Version = 1;
}
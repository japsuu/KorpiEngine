using KorpiEngine.Core.Entities.Components;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Entities;

/// <summary>
/// Basic entity with a transform component.
/// </summary>
public abstract class TransformEntity : Entity
{
    public readonly TransformEntityComponent Transform;


    protected TransformEntity(Vector3 localPosition = default)
    {
        Transform = AddComponent<TransformEntityComponent>();
        
        Transform.LocalPosition = localPosition;
    }
}
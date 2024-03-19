using KorpiEngine.Core.ECS;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Scripting.Components;

/// <summary>
/// Contains the position, rotation and scale of an <see cref="Entity"/>.
/// </summary>
public class Transform : Component
{
    internal override Type NativeComponentType => typeof(TransformComponent);


    public Vector3 Position
    {
        get => Entity.GetNativeComponent<TransformComponent>().Position;
        set => Entity.GetNativeComponent<TransformComponent>().Position = value;
    }

    public Quaternion Rotation
    {
        get => Entity.GetNativeComponent<TransformComponent>().Rotation;
        set => Entity.GetNativeComponent<TransformComponent>().Rotation = value;
    }

    public Vector3 Scale
    {
        get => Entity.GetNativeComponent<TransformComponent>().Scale;
        set => Entity.GetNativeComponent<TransformComponent>().Scale = value;
    }
    
    public Vector3 EulerAngles
    {
        get => Entity.GetNativeComponent<TransformComponent>().EulerAngles;
        set => Entity.GetNativeComponent<TransformComponent>().EulerAngles = value;
    }
    
    public Vector3 Forward => Entity.GetNativeComponent<TransformComponent>().Forward;
    
    public Vector3 Up => Entity.GetNativeComponent<TransformComponent>().Up;
    
    public Vector3 Right => Entity.GetNativeComponent<TransformComponent>().Right;
    
    public Matrix4 Matrix
    {
        get => Entity.GetNativeComponent<TransformComponent>().Transform;
        set => Entity.GetNativeComponent<TransformComponent>().Transform = value;
    }
    
    
    public void Translate(Vector3 translation, Space relativeTo = Space.Self)
    {
        Position += relativeTo switch
        {
            Space.World => translation,
            Space.Self => TransformDirection(translation),
            _ => throw new ArgumentOutOfRangeException(nameof(relativeTo), relativeTo, null)
        };
    }
    
    
    /// <summary>
    /// Transforms direction from local space to world space.
    /// This operation is not affected by scale or position of the transform. The returned vector has the same length as the original vector.
    /// </summary>
    /// <param name="direction">The direction to transform.</param>
    /// <returns>The transformed direction.</returns>
    public Vector3 TransformDirection(Vector3 direction)
    {
        // Transform the direction from local space to world space
        return Vector3.TransformVector(direction, Matrix);
    }
    
    
    /// <summary>
    /// Transforms a direction from world space to local space. The opposite of <see cref="TransformDirection"/>.
    /// This operation is not affected by scale or position of the transform. The returned vector has the same length as the original vector.
    /// </summary>
    /// <param name="direction">The direction to transform.</param>
    /// <returns>The transformed direction.</returns>
    public Vector3 InverseTransformDirection(Vector3 direction)
    {
        // Transform the direction from world space to local space
        Matrix4 inverseMatrix = Matrix4.Invert(Matrix);
        return Vector3.TransformVector(direction, inverseMatrix);
    }
    
    
    /// <summary>
    /// Transforms position from local space to world space.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 TransformPoint(Vector3 position)
    {
        return Vector3.TransformPosition(position, Matrix);
    }
    
    
    /// <summary>
    /// Transforms a position from world space to local space. The opposite of <see cref="TransformPoint"/>.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 InverseTransformPoint(Vector3 position)
    {
        Matrix4 inverseMatrix = Matrix4.Invert(Matrix);
        return Vector3.TransformPosition(position, inverseMatrix);
    }
}
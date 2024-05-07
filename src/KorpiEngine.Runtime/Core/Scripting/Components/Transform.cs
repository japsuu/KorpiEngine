using KorpiEngine.Core.API;
using KorpiEngine.Core.ECS;

namespace KorpiEngine.Core.Scripting.Components;

/// <summary>
/// Contains the position, rotation and scale of an <see cref="Entity"/>.
/// </summary>
public class Transform : Component
{
    internal override Type NativeComponentType => typeof(TransformComponent);


    /// <summary>
    /// Position of the transform.
    /// </summary>
    public Vector3 Position
    {
        get => Entity.GetNativeComponent<TransformComponent>().Position;
        set => Entity.GetNativeComponent<TransformComponent>().Position = value;
    }

    /// <summary>
    /// Rotation of the transform as a quaternion.
    /// Korpi uses a left-handed coordinate system, so positive rotation is clockwise about the axis of rotation when the axis points toward you.
    /// Read more at: https://www.evl.uic.edu/ralph/508S98/coordinates.html
    /// </summary>
    public Quaternion Rotation
    {
        get => Entity.GetNativeComponent<TransformComponent>().Rotation;
        set => Entity.GetNativeComponent<TransformComponent>().Rotation = value;
    }

    /// <summary>
    /// Scale of the transform.
    /// </summary>
    public Vector3 Scale
    {
        get => Entity.GetNativeComponent<TransformComponent>().Scale;
        set => Entity.GetNativeComponent<TransformComponent>().Scale = value;
    }

    /// <summary>
    /// <see cref="Rotation"/> of the transform as Euler angles in degrees.
    /// Korpi uses a left-handed coordinate system, so positive rotation is clockwise about the axis of rotation when the axis points toward you.
    /// Read more at: https://www.evl.uic.edu/ralph/508S98/coordinates.html
    /// </summary>
    public Vector3 EulerAngles
    {
        get => Entity.GetNativeComponent<TransformComponent>().EulerAngles;
        set => Entity.GetNativeComponent<TransformComponent>().EulerAngles = value;
    }
    
    public Vector3 Forward => Entity.GetNativeComponent<TransformComponent>().Forward;
    
    public Vector3 Up => Entity.GetNativeComponent<TransformComponent>().Up;
    
    public Vector3 Right => Entity.GetNativeComponent<TransformComponent>().Right;
    
    public Matrix4x4 Matrix
    {
        get => Entity.GetNativeComponent<TransformComponent>().Matrix;
        set => Entity.GetNativeComponent<TransformComponent>().Matrix = value;
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
    
    
    public void Rotate(Vector3 eulerAngles, Space relativeTo = Space.Self)
    {
        Quaternion rotation = Quaternion.Euler(eulerAngles);
        Rotate(rotation, relativeTo);
    }


    public void Rotate(Quaternion rotation, Space relativeTo)
    {
        Quaternion newRotation = relativeTo switch
        {
            Space.World => rotation * Rotation,
            Space.Self => Rotation * rotation,
            _ => throw new ArgumentOutOfRangeException(nameof(relativeTo), relativeTo, null)
        };
        Rotation = newRotation;
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
        return Vector3.TransformNormal(direction, Matrix);
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
        Matrix4x4.Invert(Matrix, out Matrix4x4 inverseMatrix);
        return Vector3.TransformNormal(direction, inverseMatrix);
    }
    
    
    /// <summary>
    /// Transforms position from local space to world space.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 TransformPoint(Vector3 position)
    {
        return Vector3.Transform(position, Matrix);
    }
    
    
    /// <summary>
    /// Transforms a position from world space to local space. The opposite of <see cref="TransformPoint"/>.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 InverseTransformPoint(Vector3 position)
    {
        Matrix4x4.Invert(Matrix, out Matrix4x4 inverseMatrix);
        return Vector3.Transform(position, inverseMatrix);
    }
}
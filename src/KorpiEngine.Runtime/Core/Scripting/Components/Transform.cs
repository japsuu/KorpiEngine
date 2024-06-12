using KorpiEngine.Core.API;
using KorpiEngine.Core.ECS;

namespace KorpiEngine.Core.Scripting.Components;

/// <summary>
/// Contains the position, rotation and scale of an <see cref="Entity"/>.
/// </summary>
public sealed class Transform : Component
{
    internal override Type NativeComponentType => typeof(TransformComponent);

    public Vector3 Right => Rotation * Vector3.Right;
    public Vector3 Left => Rotation * Vector3.Left;
    public Vector3 Up => Rotation * Vector3.Up;
    public Vector3 Down => Rotation * Vector3.Down;
    public Vector3 Forward => Rotation * Vector3.Forward;
    public Vector3 Backward => Rotation * Vector3.Backward;

    /// <summary>
    /// Used to check if the transform has changed since the last time it was accessed.
    /// https://forum.unity.com/threads/transform-haschanged-would-be-better-if-replaced-by-a-version-number.700004/
    /// </summary>
    public uint Version
    {
        get => Entity.GetNativeComponent<TransformComponent>().Version;
        private set => Entity.GetNativeComponent<TransformComponent>().Version = value;
    }


    #region Reference Getters and Setters

    // These reference getters & setters should get inlined away by the JIT compiler
    private ref Vector3 LocalPositionRead => ref Entity.GetNativeComponent<TransformComponent>().LocalPosition;

    private Vector3 LocalPositionWrite
    {
        set => Entity.GetNativeComponent<TransformComponent>().LocalPosition = value;
    }

    private ref Quaternion LocalRotationRead => ref Entity.GetNativeComponent<TransformComponent>().LocalRotation;

    private Quaternion LocalRotationWrite
    {
        set => Entity.GetNativeComponent<TransformComponent>().LocalRotation = value;
    }

    private ref Vector3 LocalScaleRead => ref Entity.GetNativeComponent<TransformComponent>().LocalScale;

    private Vector3 LocalScaleWrite
    {
        set => Entity.GetNativeComponent<TransformComponent>().LocalScale = value;
    }

    #endregion


    #region Parenting

    public Transform? Parent
    {
        get => Entity.GetNativeComponent<TransformComponent>().Parent;
        private set => Entity.GetNativeComponent<TransformComponent>().Parent = value;
    }
    
    
    public List<Transform> Children => Entity.GetNativeComponent<TransformComponent>().Children;


    public bool SetParent(Transform? newParent, bool worldPositionStays = true)
    {
        if (newParent == Parent)
            return true;

        // Make sure that the new father is not a child of this transform.
        if (IsChildOrSameTransform(newParent, this))
            return false;

        // Save the old position in world space
        Vector3 worldPosition = new();
        Quaternion worldRotation = new();
        Matrix4x4 worldScale = new();

        if (worldPositionStays)
        {
            worldPosition = Transform.Position;
            worldRotation = Transform.Rotation;
            worldScale = Transform.GetWorldRotationAndScale();
        }

        if (newParent != Parent)
        {
            Parent?.Children.Remove(this);
            newParent?.Children.Add(this);

            Parent = newParent;
        }

        if (worldPositionStays)
        {
            if (Parent != null)
            {
                Transform.LocalPosition = Parent.Transform.InverseTransformPoint(worldPosition);
                Transform.LocalRotation = Quaternion.NormalizeSafe(Quaternion.Inverse(Parent.Transform.Rotation) * worldRotation);
            }
            else
            {
                Transform.LocalPosition = worldPosition;
                Transform.LocalRotation = Quaternion.NormalizeSafe(worldRotation);
            }

            Transform.LocalScale = Vector3.One;
            Matrix4x4 inverseRs = Transform.GetWorldRotationAndScale().Invert() * worldScale;
            Transform.LocalScale = new Vector3(inverseRs[0, 0], inverseRs[1, 1], inverseRs[2, 2]);
        }

        HierarchyStateChanged();

        return true;
    }


    public static bool IsChildOrSameTransform(Transform? transform, Transform inParent)
    {
        Transform? child = transform;
        while (child != null)
        {
            if (child == inParent)
                return true;
            child = child.Parent;
        }

        return false;
    }


    public bool IsChildOf(Entity parent)
    {
        if (InstanceID == parent.InstanceID)
            return false; // Not a child, they're the same object

        Entity? child = Entity;
        while (child != null)
        {
            if (child == parent)
                return true;
            child = child.Parent;
        }

        return false;
    }


    private void HierarchyStateChanged()
    {
        bool newState = Entity.Enabled && Entity.IsParentEnabled();
        Entity.EnabledInHierarchy = newState;

        foreach (Transform child in Children)
            child.HierarchyStateChanged();
    }

    #endregion


    #region Position

    /// <summary>
    /// Local position of the transform.
    /// </summary>
    public Vector3 LocalPosition
    {
        get => MakeSafe(LocalPositionRead);
        set
        {
            if (LocalPositionRead == value)
                return;

            LocalPositionWrite = MakeSafe(value);
            Version++;
        }
    }

    public Vector3 Position
    {
        get
        {
            if (Parent != null)
                return MakeSafe(Parent.LocalToWorldMatrix.MultiplyPoint(LocalPosition));
            return MakeSafe(LocalPosition);
        }
        set
        {
            Vector3 newPosition = value;
            if (Parent != null)
                newPosition = Parent.InverseTransformPoint(newPosition);

            LocalPosition = MakeSafe(newPosition);
            Version++;
        }
    }


    public static Vector3 GetPosition(TransformComponent component)
    {
        if (component.Parent != null)
            return MakeSafe(component.Parent.LocalToWorldMatrix.MultiplyPoint(component.LocalPosition));
        return component.LocalPosition;
    }

    #endregion


    #region Rotation

    /// <summary>
    /// Local rotation of the transform as a quaternion.
    /// Korpi uses a left-handed coordinate system, so positive rotation is clockwise about the axis of rotation when the axis points toward you.
    /// Read more at: https://www.evl.uic.edu/ralph/508S98/coordinates.html
    /// </summary>
    public Quaternion LocalRotation
    {
        get => MakeSafe(LocalRotationRead);
        set
        {
            if (LocalRotationRead == value)
                return;

            LocalRotationWrite = MakeSafe(value);
            Version++;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            Quaternion worldRot = LocalRotation;
            Transform? p = Parent;
            while (p != null)
            {
                worldRot = p.LocalRotation * worldRot;
                p = p.Parent;
            }

            return MakeSafe(worldRot);
        }
        private set
        {
            if (Parent != null)
                LocalRotation = MakeSafe(Quaternion.NormalizeSafe(Quaternion.Inverse(Parent.Rotation) * value));
            else
                LocalRotation = MakeSafe(Quaternion.NormalizeSafe(value));
            Version++;
        }
    }

    /// <summary>
    /// <see cref="Rotation"/> of the transform as Euler angles in degrees.
    /// Korpi uses a left-handed coordinate system, so positive rotation is clockwise about the axis of rotation when the axis points toward you.
    /// Read more at: https://www.evl.uic.edu/ralph/508S98/coordinates.html
    /// </summary>
    public Vector3 EulerAngles
    {
        get => MakeSafe(Rotation.EulerAngles);
        set
        {
            Rotation = MakeSafe(Quaternion.Euler(value));
            Version++;
        }
    }

    public Vector3 LocalEulerAngles
    {
        get => MakeSafe(LocalRotation.EulerAngles);
        set
        {
            LocalRotationRead.EulerAngles = MakeSafe(value);
            Version++;
        }
    }
    
    
    public static Quaternion GetRotation(TransformComponent component)
    {
        Quaternion worldRot = component.LocalRotation;
        Transform? p = component.Parent;
        while (p != null)
        {
            worldRot = p.LocalRotation * worldRot;
            p = p.Parent;
        }

        return MakeSafe(worldRot);
    }

    #endregion


    #region Scale

    /// <summary>
    /// Local scale of the transform.
    /// </summary>
    public Vector3 LocalScale
    {
        get => MakeSafe(LocalScaleRead);
        set
        {
            if (LocalScaleRead == value)
                return;

            LocalScaleWrite = MakeSafe(value);
            Version++;
        }
    }

    public Vector3 LossyScale
    {
        get
        {
            Vector3 scale = LocalScale;
            Transform? p = Parent;
            while (p != null)
            {
                scale.Scale(p.LocalScale);
                p = p.Parent;
            }

            return MakeSafe(scale);
        }
    }
    
    
    public static Vector3 GetScale(TransformComponent component)
    {
        Vector3 scale = component.LocalScale;
        Transform? p = component.Parent;
        while (p != null)
        {
            scale.Scale(p.LocalScale);
            p = p.Parent;
        }

        return MakeSafe(scale);
    }

    #endregion


    #region Matrices

    public Matrix4x4 WorldToLocalMatrix => LocalToWorldMatrix.Invert();

    public Matrix4x4 LocalToWorldMatrix
    {
        get
        {
            Matrix4x4 t = Matrix4x4.TRS(LocalPosition, LocalRotation, LocalScale);
            return Parent != null ? t * Parent.LocalToWorldMatrix : t;
        }
    }


    public Matrix4x4 GetWorldRotationAndScale()
    {
        Matrix4x4 ret = Matrix4x4.TRS(new Vector3(0, 0, 0), Entity.GetNativeComponent<TransformComponent>().LocalRotation, LocalScale);
        if (Parent == null)
            return ret;

        Matrix4x4 parentTransform = Parent.GetWorldRotationAndScale();
        ret = parentTransform * ret;

        return ret;
    }
    
    
    public static Matrix4x4 GetMatrix(TransformComponent component)
    {
        return Matrix4x4.TRS(GetPosition(component), GetRotation(component), GetScale(component));
    }

    #endregion


    #region Transform Hierarchy Traversal

    public Transform? Find(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        string[] names = path.Split('/');
        Transform currentTransform = this;

        foreach (string name in names)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            Transform? childTransform = FindImmediateChild(currentTransform, name);
            if (childTransform == null)
                return null;

            currentTransform = childTransform;
        }

        return currentTransform;
    }


    private static Transform? FindImmediateChild(Transform parent, string name)
    {
        foreach (Transform child in parent.Children)
            if (child.Name == name)
                return child;
        return null;
    }


    public Transform? DeepFind(string? name)
    {
        if (name == null)
            return null;
        if (name == Entity.Name)
            return this;
        foreach (Transform child in Children)
        {
            Transform? t = child.DeepFind(name);
            if (t != null)
                return t;
        }

        return null;
    }


    public static string GetPath(Transform target, Transform root)
    {
        string path = target.Entity.Name;
        while (target.Parent != null)
        {
            target = target.Parent;
            path = target.Entity.Name + "/" + path;
            if (target == root)
                break;
        }

        return path;
    }

    #endregion


    #region Transformations

    public void Translate(Vector3 translation, Transform? relativeTo = null)
    {
        if (relativeTo != null)
            Position += relativeTo.TransformDirection(translation);
        else
            Position += translation;
    }


    public void Rotate(Vector3 eulerAngles, bool relativeToSelf = true)
    {
        Quaternion eulerRot = Quaternion.Euler(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        if (relativeToSelf)
            LocalRotation *= eulerRot;
        else
            Rotation *= Quaternion.Inverse(Rotation) * eulerRot * Rotation;
    }


    public void Rotate(Vector3 axis, double angle, bool relativeToSelf = true)
    {
        RotateAroundInternal(relativeToSelf ? TransformDirection(axis) : axis, angle * Maths.DEG_2_RAD);
    }


    public void RotateAround(Vector3 point, Vector3 axis, double angle)
    {
        Vector3 worldPos = Position;
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        Vector3 dif = worldPos - point;
        dif = q * dif;
        worldPos = point + dif;
        Position = worldPos;
        RotateAroundInternal(axis, angle * Maths.DEG_2_RAD);
    }


    private void RotateAroundInternal(Vector3 worldAxis, double rad)
    {
        Vector3 localAxis = InverseTransformDirection(worldAxis);
        if (localAxis.SqrMagnitude > Maths.EPSILON)
        {
            localAxis.Normalize();
            Quaternion q = Quaternion.AngleAxis(rad, localAxis);
            LocalRotation = Quaternion.NormalizeSafe(LocalRotation * q);
        }
    }


    public void LookAt(Vector3 worldPosition, Vector3 worldUp)
    {
        // Cheat using Matrix4x4.CreateLookAt
        Matrix4x4 m = Matrix4x4.CreateLookAt(Position, worldPosition, worldUp);
        LocalRotation = Quaternion.NormalizeSafe(Quaternion.MatrixToQuaternion(m));
    }


    /// <summary>
    /// Transforms direction from local space to world space.
    /// This operation is not affected by scale or position of the transform. The returned vector has the same length as the original vector.
    /// </summary>
    /// <param name="direction">The direction to transform.</param>
    /// <returns>The transformed direction.</returns>
    public Vector3 TransformDirection(Vector3 direction) => Rotation * direction;


    /// <summary>
    /// Transforms a direction from world space to local space. The opposite of <see cref="TransformDirection"/>.
    /// This operation is not affected by scale or position of the transform. The returned vector has the same length as the original vector.
    /// </summary>
    /// <param name="direction">The direction to transform.</param>
    /// <returns>The transformed direction.</returns>
    public Vector3 InverseTransformDirection(Vector3 direction) => Quaternion.Inverse(Rotation) * direction;


    /// <summary>
    /// Transforms position from local space to world space.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 TransformPoint(Vector3 position) => Vector4.Transform(new Vector4(position, 1.0), LocalToWorldMatrix).Xyz;


    /// <summary>
    /// Transforms a position from world space to local space. The opposite of <see cref="TransformPoint"/>.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 InverseTransformPoint(Vector3 position) => Vector4.Transform(new Vector4(position, 1.0), WorldToLocalMatrix).Xyz;


    public Vector3 TransformVector(Vector3 inVector)
    {
        Vector3 worldVector = inVector;

        Transform? cur = this;
        while (cur != null)
        {
            worldVector.Scale(cur.LocalScale);
            worldVector = cur.LocalRotation * worldVector;

            cur = cur.Parent;
        }

        return worldVector;
    }


    public Vector3 InverseTransformVector(Vector3 inVector)
    {
        Vector3 localVector;
        if (Parent != null)
            localVector = Parent.InverseTransformVector(inVector);
        else
            localVector = inVector;

        Vector3 newVector = Quaternion.Inverse(LocalRotation) * localVector;
        if (LocalScale != Vector3.One)
            newVector.Scale(InverseSafe(LocalScale));

        return newVector;
    }


    public Quaternion TransformRotation(Quaternion inRotation)
    {
        Quaternion worldRotation = inRotation;

        Transform? cur = this;
        while (cur != null)
        {
            worldRotation = cur.LocalRotation * worldRotation;
            cur = cur.Parent;
        }

        return worldRotation;
    }


    public Quaternion InverseTransformRotation(Quaternion worldRotation) => Quaternion.Inverse(Rotation) * worldRotation;

    #endregion


    private static double MakeSafe(double v) => double.IsNaN(v) ? 0 : v;
    private static Vector3 MakeSafe(Vector3 v) => new(MakeSafe(v.X), MakeSafe(v.Y), MakeSafe(v.Z));
    private static Quaternion MakeSafe(Quaternion v) => new(MakeSafe(v.X), MakeSafe(v.Y), MakeSafe(v.Z), MakeSafe(v.W));
    private static double InverseSafe(double f) => Math.Abs(f) > Maths.EPSILON ? 1.0F / f : 0.0F;
    private static Vector3 InverseSafe(Vector3 v) => new(InverseSafe(v.X), InverseSafe(v.Y), InverseSafe(v.Z));
}
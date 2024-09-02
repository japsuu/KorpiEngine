namespace KorpiEngine.EntityModel.SpatialHierarchy;

public class Transform
{
    public Vector3 Right => Rotation * Vector3.Right;
    public Vector3 Left => Rotation * Vector3.Left;
    public Vector3 Up => Rotation * Vector3.Up;
    public Vector3 Down => Rotation * Vector3.Down;
    public Vector3 Backward => Rotation * Vector3.Backward;
    public Vector3 Forward
    {
        get => Rotation * Vector3.Forward;
        set => Rotation = Quaternion.LookAtDirection(value, Vector3.Up);
    }

    public bool IsRootTransform => Parent == null;
    public Transform Root => Parent == null ? this : Parent.Root;

    /// <summary>
    /// Used to check if the transform has changed since the last time it was accessed.
    /// https://forum.unity.com/threads/transform-haschanged-would-be-better-if-replaced-by-a-version-number.700004/
    /// </summary>
    public uint Version { get; private set; }
    
    /// <summary>
    /// The entity that this transform is attached to.
    /// </summary>
    public Entity Entity { get; internal set; } = null!;

    private Vector3 _localPosition = Vector3.Zero;
    private Vector3 _localScale = Vector3.One;
    private Quaternion _localRotation = Quaternion.Identity;

    public Transform? Parent => Entity.Parent?.Transform;


    #region Position

    /// <summary>
    /// Local position of the transform.
    /// </summary>
    public Vector3 LocalPosition
    {
        get => MakeSafe(_localPosition);
        set
        {
            if (_localPosition == value)
                return;

            _localPosition = MakeSafe(value);
            Version++;
        }
    }

    public Vector3 Position
    {
        get
        {
            if (Parent != null)
                return MakeSafe(Parent.TransformPoint(LocalPosition));
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
    
    #endregion


    #region Rotation

    /// <summary>
    /// Local rotation of the transform as a quaternion.
    /// Korpi uses a left-handed coordinate system, so positive rotation is clockwise about the axis of rotation when the axis points toward you.
    /// Read more at: https://www.evl.uic.edu/ralph/508S98/coordinates.html
    /// </summary>
    public Quaternion LocalRotation
    {
        get => MakeSafe(_localRotation);
        set
        {
            if (_localRotation == value)
                return;

            _localRotation = MakeSafe(value);
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
        set
        {
            if (Parent != null)
                LocalRotation = MakeSafe((Parent.Rotation.Inverse() * value).NormalizeSafe());
            else
                LocalRotation = MakeSafe(value.NormalizeSafe());
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
        get => MakeSafe(Rotation.ToEulerAngles().ToDegrees());
        set
        {
            Rotation = MakeSafe(Quaternion.CreateFromEulerAngles(value.ToRadians()));
            Version++;
        }
    }

    public Vector3 LocalEulerAngles
    {
        get => MakeSafe(_localRotation.ToEulerAngles().ToDegrees());
        set
        {
            _localRotation = MakeSafe(Quaternion.CreateFromEulerAngles(value.ToRadians()));
            Version++;
        }
    }

    #endregion


    #region Scale

    /// <summary>
    /// Local scale of the transform.
    /// </summary>
    public Vector3 LocalScale
    {
        get => MakeSafe(_localScale);
        set
        {
            if (_localScale == value)
                return;

            _localScale = MakeSafe(value);
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

    #endregion


    #region Matrices

    public Matrix4x4 WorldToLocalMatrix => LocalToWorldMatrix.Inverse();

    public Matrix4x4 LocalToWorldMatrix
    {
        get
        {
            Matrix4x4 t = Matrix4x4.CreateTRS(LocalPosition, LocalRotation, LocalScale);
            return Parent != null ? t * Parent.LocalToWorldMatrix : t;
        }
    }


    public Matrix4x4 GetWorldRotationAndScale()
    {
        Matrix4x4 ret = Matrix4x4.CreateTRS(Vector3.Zero, LocalRotation, LocalScale);
        if (Parent == null)
            return ret;

        Matrix4x4 parentTransform = Parent.GetWorldRotationAndScale();
        ret = parentTransform * ret;

        return ret;
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
        Quaternion eulerRot = Quaternion.CreateFromEulerAngles(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        if (relativeToSelf)
            LocalRotation *= eulerRot;
        else
            Rotation *= Rotation.Inverse() * eulerRot * Rotation;
    }


    public void Rotate(Vector3 axis, float angle, bool relativeToSelf = true)
    {
        RotateAroundInternal(relativeToSelf ? TransformDirection(axis) : axis, angle.ToRadians());
    }


    public void RotateAround(Vector3 point, Vector3 axis, float angle)
    {
        Vector3 worldPos = Position;
        Quaternion q = Quaternion.CreateFromAxisAngle(axis, angle);
        Vector3 dif = worldPos - point;
        dif = q * dif;
        worldPos = point + dif;
        Position = worldPos;
        RotateAroundInternal(axis, angle.ToRadians());
    }


    private void RotateAroundInternal(Vector3 worldAxis, float rad)
    {
        Vector3 localAxis = InverseTransformDirection(worldAxis);
        if (localAxis.MagnitudeSquared() > MathOps.EPSILON_FLOAT)
        {
            localAxis.Normalize();
            Quaternion q = Quaternion.CreateFromAxisAngle(localAxis, rad);
            LocalRotation = (LocalRotation * q).NormalizeSafe();
        }
    }


    public void LookAt(Vector3 worldPosition, Vector3 worldUp)
    {
        // Cheat using Matrix4x4.CreateLookAt
        Matrix4x4 m = Matrix4x4.CreateLookAt(Position, worldPosition, worldUp);
        LocalRotation = Quaternion.CreateFromRotationMatrix(m).NormalizeSafe();
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
    public Vector3 InverseTransformDirection(Vector3 direction) => Rotation.Inverse() * direction;


    /// <summary>
    /// Transforms position from local space to world space.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 TransformPoint(Vector3 position) => new Vector4(position, 1.0f).Transform(LocalToWorldMatrix).XYZ;


    /// <summary>
    /// Transforms a position from world space to local space. The opposite of <see cref="TransformPoint"/>.
    /// Note that the returned position is affected by scale.
    /// </summary>
    /// <param name="position">The position to transform.</param>
    /// <returns>The transformed position.</returns>
    public Vector3 InverseTransformPoint(Vector3 position) => new Vector4(position, 1.0f).Transform(WorldToLocalMatrix).XYZ;


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

        Vector3 newVector = LocalRotation.Inverse() * localVector;
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


    public Quaternion InverseTransformRotation(Quaternion worldRotation) => Rotation.Inverse() * worldRotation;

    #endregion


    #region Hierarchy

    public Transform? Find(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string[] names = path.Split('/');
        Transform currentTransform = this;

        foreach (string name in names)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            Transform? childTransform = FindImmediateChild(currentTransform, name);
            if (childTransform == null)
                return null;

            currentTransform = childTransform;
        }

        return currentTransform;
    }


    public Transform? DeepFind(string? name)
    {
        if (name == null)
            return null;
        
        if (name == Entity.Name)
            return this;
        
        foreach (Entity child in Entity.Children)
        {
            Transform? t = child.Transform.DeepFind(name);
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
            path = $"{target.Entity.Name}/{path}";
            if (target == root)
                break;
        }

        return path;
    }


    private static Transform? FindImmediateChild(Transform parent, string name)
    {
        foreach (Entity child in parent.Entity.Children)
            if (child.Name == name)
                return child.Transform;
        
        return null;
    }

    #endregion


    private static float MakeSafe(float v) => double.IsNaN(v) ? 0 : v;
    private static Vector3 MakeSafe(Vector3 v) => new(MakeSafe(v.X), MakeSafe(v.Y), MakeSafe(v.Z));
    private static Quaternion MakeSafe(Quaternion v) => new(MakeSafe(v.X), MakeSafe(v.Y), MakeSafe(v.Z), MakeSafe(v.W));
    private static float InverseSafe(float f) => Math.Abs(f) > MathOps.EPSILON_FLOAT ? 1.0F / f : 0.0F;
    private static Vector3 InverseSafe(Vector3 v) => new(InverseSafe(v.X), InverseSafe(v.Y), InverseSafe(v.Z));
}
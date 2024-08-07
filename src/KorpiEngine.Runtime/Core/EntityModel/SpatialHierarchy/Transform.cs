﻿using KorpiEngine.Core.API;

namespace KorpiEngine.Core.EntityModel.SpatialHierarchy;

public class Transform
{
    public Vector3 Right => Rotation * Vector3.Right;
    public Vector3 Left => Rotation * Vector3.Left;
    public Vector3 Up => Rotation * Vector3.Up;
    public Vector3 Down => Rotation * Vector3.Down;
    public Vector3 Forward => Rotation * Vector3.Forward;
    public Vector3 Backward => Rotation * Vector3.Backward;
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
        get => MakeSafe(_localRotation.EulerAngles);
        set
        {
            _localRotation.EulerAngles = MakeSafe(value);
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
        Matrix4x4 ret = Matrix4x4.TRS(Vector3.Zero, LocalRotation, LocalScale);
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
        Quaternion eulerRot = Quaternion.Euler(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        if (relativeToSelf)
            LocalRotation *= eulerRot;
        else
            Rotation *= Quaternion.Inverse(Rotation) * eulerRot * Rotation;
    }


    public void Rotate(Vector3 axis, double angle, bool relativeToSelf = true)
    {
        RotateAroundInternal(relativeToSelf ? TransformDirection(axis) : axis, angle * Mathd.DEG_2_RAD);
    }


    public void RotateAround(Vector3 point, Vector3 axis, double angle)
    {
        Vector3 worldPos = Position;
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        Vector3 dif = worldPos - point;
        dif = q * dif;
        worldPos = point + dif;
        Position = worldPos;
        RotateAroundInternal(axis, angle * Mathd.DEG_2_RAD);
    }


    private void RotateAroundInternal(Vector3 worldAxis, double rad)
    {
        Vector3 localAxis = InverseTransformDirection(worldAxis);
        if (localAxis.SqrMagnitude > Mathd.EPSILON)
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
    private static double InverseSafe(double f) => Math.Abs(f) > Mathd.EPSILON ? 1.0F / f : 0.0F;
    private static Vector3 InverseSafe(Vector3 v) => new(InverseSafe(v.X), InverseSafe(v.Y), InverseSafe(v.Z));
}
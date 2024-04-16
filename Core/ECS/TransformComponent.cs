using OpenTK.Mathematics;

namespace KorpiEngine.Core.ECS;

public struct TransformComponent : INativeComponent
{
    public Matrix4 Transform;

    public Vector3 Position
    {
        get => Transform.ExtractTranslation();
        set
        {
            // The translation is stored in the matrix at Row3.Xyz
            Transform.M41 = value.X;
            Transform.M42 = value.Y;
            Transform.M43 = value.Z;
        }
    }

    public Quaternion Rotation
    {
        get => Transform.ExtractRotation();
        set
        {
            throw new NotImplementedException();
        }
    }

    public Vector3 Scale
    {
        get => Transform.ExtractScale();
        set
        {
            // The Transform.ExtractScale() method seems to internally call this code:
            // Vector3 xyz = this.Row0.Xyz;
            // double length1 = (double) xyz.Length;
            // xyz = this.Row1.Xyz;
            // double length2 = (double) xyz.Length;
            // xyz = this.Row2.Xyz;
            // double length3 = (double) xyz.Length;
            // return new Vector3((float) length1, (float) length2, (float) length3);
            
            // This means that the scale is stored in the matrix at Row0.Xyz, Row1.Xyz and Row2.Xyz
            Transform.M11 = value.X;
            Transform.M22 = value.Y;
            Transform.M33 = value.Z;
        }
    }

    public Vector3 EulerAngles
    {
        get => Transform.ExtractRotation().ToEulerAngles();
        set
        {
            throw new NotImplementedException();
        }
    }

    public Vector3 Forward
    {
        get
        {
            Vector3 vec;
            vec.X = -Transform.M31;
            vec.Y = -Transform.M32;
            vec.Z = -Transform.M33;
            return vec.Normalized();
        }
    }

    public Vector3 Up
    {
        get
        {
            Vector3 vec;
            vec.X = Transform.M21;
            vec.Y = Transform.M22;
            vec.Z = Transform.M23;
            return vec.Normalized();
        }
    }

    public Vector3 Right
    {
        get
        {
            Vector3 vec;
            vec.X = Transform.M11;
            vec.Y = Transform.M12;
            vec.Z = Transform.M13;
            return vec.Normalized();
        }
    }


    // Implicit conversion to Matrix4.
    public static implicit operator Matrix4(TransformComponent t) => t.Transform;
}
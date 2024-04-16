using OpenTK.Mathematics;

namespace KorpiEngine.Core.ECS;

public struct TransformComponent : INativeComponent
{
    public Matrix4 Transform;


    public TransformComponent()
    {
        Transform = Matrix4.Identity;
    }
    

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
            // Create a rotation matrix from the provided quaternion
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(value);

            // Extract the current translation from the Transform
            Vector3 translation = Transform.ExtractTranslation();

            // Extract the current scale from the Transform
            Vector3 scale = Transform.ExtractScale();

            // Combine the rotation, translation and scale back into the Transform
            Transform = Matrix4.CreateScale(scale) * rotationMatrix * Matrix4.CreateTranslation(translation);
        }
    }

    public Vector3 Scale
    {
        get => Transform.ExtractScale();
        set
        {
            // The Transform.ExtractScale() method calls internally this code:
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
        get
        {
            Vector3 eulerAnglesInRadians = Rotation.ToEulerAngles();
            Vector3 eulerAnglesInDegrees = eulerAnglesInRadians * (180.0f / MathF.PI);
            return eulerAnglesInDegrees;
        }
        set
        {
            Vector3 eulerAnglesInRadians = value * (MathF.PI / 180.0f);
            Rotation = Quaternion.FromEulerAngles(eulerAnglesInRadians);
        }
    }

    public Vector3 EulerAnglesRadians
    {
        get => Rotation.ToEulerAngles();
        set => Rotation = Quaternion.FromEulerAngles(value);
    }

    public Vector3 Forward
    {
        get
        {
            // As we are using a right-handed coordinate system, the forward vector is the negated Z-axis
            Vector3 vector = new(-Transform.M31, -Transform.M32, -Transform.M33);
            return vector.Normalized();
        }
    }

    public Vector3 Up
    {
        get
        {
            // The up vector is the Y-axis
            Vector3 vector = new(Transform.M21, Transform.M22, Transform.M23);
            return vector.Normalized();
        }
    }

    public Vector3 Right
    {
        get
        {
            Vector3 vector = new(Transform.M11, Transform.M12, Transform.M13);
            return vector.Normalized();
        }
    }


    // Implicit conversion to Matrix4.
    public static implicit operator Matrix4(TransformComponent t) => t.Transform;
}
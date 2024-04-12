using KorpiEngine.Core.ECS.Systems;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.Scripting;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.ECS;

/// <summary>
/// Represents a native data-only component directly attached to an <see cref="Entity"/>.
/// These components are processed by <see cref="NativeSystem"/>s.
/// </summary>
internal interface INativeComponent
{
}


/// <summary>
/// Contains a list of attached <see cref="Behaviour"/>s.
/// </summary>
public struct BehaviourComponent : INativeComponent
{
    public List<Behaviour>? Behaviours;
}


public struct IdComponent : INativeComponent
{
    public UUID Id;
}


public struct NameComponent : INativeComponent
{
    public string Name;
}


public struct SpriteRendererComponent : INativeComponent
{
    public Color Color;
}


public struct MeshRendererComponent : INativeComponent
{
    public Mesh? Mesh;
    public Material? Material;
}


public struct TransformComponent : INativeComponent
{
    public Matrix4 Transform;

    public Vector3 Position
    {
        get => Transform.ExtractTranslation();
        set
        {
            Transform.ClearTranslation();
            Transform *= Matrix4.CreateTranslation(value);
        }
    }

    public Quaternion Rotation
    {
        get => Transform.ExtractRotation();
        set
        {
            Transform.ClearRotation();
            Transform *= Matrix4.CreateFromQuaternion(value);
        }
    }

    public Vector3 Scale
    {
        get => Transform.ExtractScale();
        set
        {
            Transform.ClearScale();
            Transform *= Matrix4.CreateScale(value);
        }
    }

    public Vector3 EulerAngles
    {
        get => Transform.ExtractRotation().ToEulerAngles();
        set
        {
            Transform.ClearRotation();
            Transform *= Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(value));
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
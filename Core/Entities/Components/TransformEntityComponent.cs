using OpenTK.Mathematics;

namespace KorpiEngine.Core.Entities.Components;

public class TransformEntityComponent : EntityComponent
{
    /// <summary>
    /// The parent of this transform, if any.
    /// </summary>
    public TransformEntityComponent? Parent { get; private set; }
    
    /// <summary>
    /// All children of this transform.
    /// All these have this transform as their parent.
    /// </summary>
    public readonly List<TransformEntityComponent> Children = new();

    /// <summary>
    /// Absolute position of this transform in the world.
    /// </summary>
    public Vector3 WorldPosition
    {
        get
        {
            if (Parent != null)
                return Parent.WorldPosition + LocalPosition;
            return LocalPosition;
        }
        set
        {
            if (Parent != null)
                LocalPosition = value - Parent.WorldPosition;
            else
                LocalPosition = value;
        }
    }
    
    /// <summary>
    /// Absolute rotation of this transform in the world (radians).
    /// </summary>
    public Vector3 WorldRotation
    {
        get
        {
            if (Parent != null)
                return Parent.WorldRotation + LocalRotation;
            return LocalRotation;
        }
        set
        {
            if (Parent != null)
                LocalRotation = value - Parent.WorldRotation;
            else
                LocalRotation = value;
        }
    }
    
    /// <summary>
    /// Position of this transform relative to its parent.
    /// </summary>
    public Vector3 LocalPosition;
    
    /// <summary>
    /// Rotation of this transform relative to its parent (radians).
    /// </summary>
    public Vector3 LocalRotation;
    
    /// <summary>
    /// Scale of this transform.
    /// </summary>
    public Vector3 Scale;


    public TransformEntityComponent()
    {
        LocalPosition = Vector3.Zero;
        LocalRotation = Vector3.Zero;
        Scale = Vector3.One;
    }
    
    
    /// <summary>
    /// Changes the parent of this transform.
    /// </summary>
    /// <param name="newParent">The new parent to set</param>
    /// <param name="keepWorldPosition">If this transform should keep it's world space position after the parent has changed</param>
    public void SetParent(TransformEntityComponent? newParent, bool keepWorldPosition = true)
    {
        if (newParent == null)
        {
            if (Parent == null)
                return;

            // Un-parent
            if (keepWorldPosition)
            {
                LocalPosition = WorldPosition;
                LocalRotation = WorldRotation;
            }
            Parent?.Children.Remove(this);
            Parent = null;
        }
        else
        {
            // Change parent
            if (Parent != null)
            {
                if (keepWorldPosition)
                {
                    LocalPosition = WorldPosition - newParent.WorldPosition;
                    LocalRotation = WorldRotation - newParent.WorldRotation;
                }
            }
            
            // Assign parent
            Parent = newParent;
        }

        Parent?.Children.Add(this);
    }


    /// <summary>
    /// Gets the model matrix of this transform, in world space.
    /// </summary>
    /// <returns>The model matrix of this transform</returns>
    public Matrix4 GetModelMatrix()
    {
        return
            Matrix4.CreateScale(Scale) *
            Matrix4.CreateRotationX(WorldRotation.X) *
            Matrix4.CreateRotationY(WorldRotation.Y) *
            Matrix4.CreateRotationZ(WorldRotation.Z) *
            Matrix4.CreateTranslation(WorldPosition);
    }
    
    
    /// <summary>
    /// Sets the local position of this transform relative to the parent.
    /// </summary>
    /// <param name="position">The new position relative to the parent</param>
    public void SetLocalPosition(Vector3 position)
    {
        LocalPosition = position;
    }
    
    
    /// <summary>
    /// Translates (moves) this transform by the provided translation.
    /// </summary>
    /// <param name="translation">The translation to apply</param>
    public void Translate(Vector3 translation)
    {
        LocalPosition += translation;
    }
    
    
    /// <summary>
    /// Rotates this transform by the provided rotation.
    /// </summary>
    /// <param name="rotationRadians">The rotation to apply (radians)</param>
    public void Rotate(Vector3 rotationRadians)
    {
        LocalRotation += rotationRadians;
    }
    
    
    /// <summary>
    /// Scales this transform by the provided scale.
    /// </summary>
    /// <param name="scale">The scale to apply</param>
    public void ScaleBy(Vector3 scale)
    {
        Scale += scale;
    }


    public override string ToString()
    {
        return $"Transform: WorldPosition={WorldPosition} (Local={LocalPosition}), WorldRotation={WorldRotation} (Local={LocalRotation}), Scale={Scale}";
    }
}
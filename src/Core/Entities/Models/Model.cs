using KorpiEngine.AssetManagement;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;

namespace KorpiEngine.Entities;

public class ModelPart
{
    public readonly string Name;
    public readonly Mesh? Mesh;
    public readonly Material? Material;
    public readonly Transform Transform = new();
    
    public ModelPart? Parent { get; private set; }
    public List<ModelPart> Children { get; } = [];

    public ModelPart(string name, Mesh? mesh, Material? material)
    {
        Name = name;
        Mesh = mesh;
        Material = material;
    }

    public void AddChild(ModelPart child)
    {
        if (child.Parent != null)
            throw new InvalidOperationException("The child already has a parent.");

        Children.Add(child);
        child.Parent = this;
    }

    public void RemoveChild(ModelPart child)
    {
        if (child.Parent != this)
            throw new InvalidOperationException("The child is not a child of this model part.");

        Children.Remove(child);
        child.Parent = null;
    }
}

/// <summary>
/// Represents a 3D-model asset, that can be thought of as an Entity prefab.<br/>
/// Contains a hierarchy of <see cref="ModelPart"/>s.<br/>
/// Can be instantiated into an <see cref="Entity"/> hierarchy by calling <see cref="CreateEntity"/>.
/// </summary>
public class Model : Asset
{
    public Model(string name) : base(name)
    {
    }


    /// <summary>
    /// Constructs a new <see cref="Entity"/> hierarchy from this model.
    /// </summary>
    /// <param name="scene">The scene to create the entity in.</param>
    /// <returns>The root entity of the created entity hierarchy.</returns>
    public Entity CreateEntity(Scene scene)
    {
        
    }
}
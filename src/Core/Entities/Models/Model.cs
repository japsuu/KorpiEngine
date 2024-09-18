using KorpiEngine.Animations;
using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;

namespace KorpiEngine.Entities;

public class ModelPart
{
    public Vector3 LocalPosition { get; set; } = Vector3.Zero;
    public Vector3 LocalScale { get; set; } = Vector3.One;
    public Quaternion LocalRotation { get; set; } = Quaternion.Identity;
    
    public string Name { get; internal set; }
    public AssetRef<Material> Material { get; internal set; }
    public AssetRef<Mesh> Mesh { get; internal set; }
    public ModelPart[]? Bones { get; internal set; }
    
    public ModelPart? Parent { get; private set; }
    public List<ModelPart> Children { get; } = [];
    
    public bool IsEmpty => !HasMesh && !HasSkinnedMesh;
    public bool HasMesh => !Mesh.ReferencesNothing;
    public bool HasSkinnedMesh => HasMesh && Bones != null;

    
    public ModelPart(string name)
    {
        Name = name;
    }

    
    public void SetParent(ModelPart parent)
    {
        Parent?.Children.Remove(this);

        Parent = parent;
        parent.Children.Add(this);
    }
    
    
    public ModelPart? DeepFind(string name)
    {
        if (Name == name)
            return this;

        foreach (ModelPart child in Children)
        {
            ModelPart? found = child.DeepFind(name);
            if (found != null)
                return found;
        }

        return null;
    }
    
    
    public void Destroy()
    {
        Parent?.Children.Remove(this);
        Parent = null;
    }
}

/// <summary>
/// Represents a 3D-model asset, that can be thought of as an Entity prefab.<br/>
/// Contains a hierarchy of <see cref="ModelPart"/>s.<br/>
/// Can be instantiated into an <see cref="Entity"/> hierarchy by calling <see cref="CreateEntity"/>.
/// </summary>
public class Model : Asset
{
    private readonly ModelPart _rootPart;
    private readonly List<AssetRef<AnimationClip>>? _animations;
    
    
    public Model(string name, ModelPart rootPart, List<AssetRef<AnimationClip>>? animations) : base(name)
    {
        _rootPart = rootPart;
        _animations = animations;
    }


    /// <summary>
    /// Constructs a new <see cref="Entity"/> hierarchy from this model.
    /// </summary>
    /// <param name="scene">The scene to create the entity in.</param>
    /// <returns>The root entity of the created entity hierarchy.</returns>
    public Entity CreateEntity(Scene scene)
    {
        List<(Entity entity, ModelPart modelPart)> modelHierarchy = [];
        Entity rootEntity = CreateEntityHierarchy(scene, _rootPart, modelHierarchy);
        
        SetupComponents(rootEntity, modelHierarchy);

        return rootEntity;
    }


    private void SetupComponents(Entity rootEntity, List<(Entity entity, ModelPart modelPart)> modelHierarchy)
    {
        // Add mesh renderer if the model part has a mesh
        foreach ((Entity entity, ModelPart modelPart) in modelHierarchy)
        {
            if (modelPart.HasMesh)
            {
                if (modelPart.HasSkinnedMesh)
                {
                    SkinnedMeshRenderer skinnedRenderer = entity.AddComponent<SkinnedMeshRenderer>();
                    skinnedRenderer.Mesh = new AssetRef<Mesh>(modelPart.Mesh.Asset!);
                    skinnedRenderer.Material = new AssetRef<Material>(modelPart.Material.Asset!);
                    
                    // Find and assign bones in the hierarchy
                    skinnedRenderer.Bones = new Transform[modelPart.Bones!.Length];
                    for (int i = 0; i < modelPart.Bones.Length; i++)
                        skinnedRenderer.Bones[i] = modelHierarchy[0].entity.Transform.DeepFind(modelPart.Bones[i].Name)!;
                }
                else
                {
                    MeshRenderer renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = new AssetRef<Mesh>(modelPart.Mesh.Asset!);
                    renderer.Material = new AssetRef<Material>(modelPart.Material.Asset!);
                }
            }
        }
        
        // Add Animation Component to root, with all the animations assigned to it.
        if (_animations != null && _animations.Count > 0)
        {
            Animation anim = rootEntity.AddComponent<Animation>();
            foreach (AssetRef<AnimationClip> a in _animations)
                anim.Clips.Add(a);
            anim.DefaultClip = _animations[0];
        }
    }


    private static Entity CreateEntityHierarchy(Scene scene, ModelPart modelPart, List<(Entity entity, ModelPart modelPart)> modelHierarchy)
    {
        Entity entity = scene.CreateEntity(modelPart.Name);
        modelHierarchy.Add((entity, modelPart));

        foreach (ModelPart child in modelPart.Children)
        {
            Entity childEntity = CreateEntityHierarchy(scene, child, modelHierarchy);
            childEntity.SetParent(entity, false);
        }
        
        entity.Transform.LocalPosition = modelPart.LocalPosition;
        entity.Transform.LocalRotation = modelPart.LocalRotation;
        entity.Transform.LocalScale = modelPart.LocalScale;

        return entity;
    }
}
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;
using KorpiEngine.Utils;
using Debug = KorpiEngine.Tools.Debug;

namespace KorpiEngine.Entities;

/// <summary>
/// Container for components and systems.
/// </summary>
public sealed partial class Entity : EngineObject
{
    private bool _isEnabled = true;
    private EntityScene? _entityScene;
    private readonly List<Entity> _childList = [];
    private readonly Transform _transform = new();
    private readonly List<EntityComponent> _components = [];
    private readonly MultiValueDictionary<Type, EntityComponent> _componentCache = new();
    private readonly Dictionary<ulong, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _systemBuckets = new();
    
    private bool IsParentEnabled => Parent == null || Parent.EnabledInHierarchy;

    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;
    internal IReadOnlyCollection<EntityComponent> Components => _components;
    
    /// <summary>
    /// The scene this entity is in.
    /// If null, the entity has not been spawned in a scene.
    /// </summary>
    public Scene? Scene { get; private set; }

    /// <summary>
    /// The transform of this entity.
    /// </summary>
    public Transform Transform
    {
        get
        {
            _transform.Entity = this;
            return _transform;
        }
    }

    /// <summary>
    /// True if this entity has no parent, false otherwise.
    /// </summary>
    public bool IsRootEntity => Parent == null;
    
    /// <summary>
    /// True if the entity is spawned in a scene, false otherwise.
    /// </summary>
    public bool IsSpawned => Scene != null;

    /// <summary>
    /// True if the entity is enabled explicitly, false otherwise.
    /// This value is unaffected by the entity's parent hierarchy.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (value != _isEnabled)
                SetEnabled(value);
        }
    }

    public Matrix4x4 GlobalCameraRelativeTransform
    {
        get
        {
            Matrix4x4 t = Transform.LocalToWorldMatrix;
            return t.SetTranslation(t.Translation - Camera.RenderingCamera.Transform.Position);
        }
    }

    /// <summary>
    /// True if the entity is enabled and all of its parents are enabled, false otherwise.
    /// </summary>
    public bool EnabledInHierarchy { get; private set; } = true;
    
    /// <summary>
    /// The hierarchical parent of this entity, or null if it is a root entity.
    /// </summary>
    public Entity? Parent { get; private set; }
    
    /// <summary>
    /// True if this entity has children, false otherwise.
    /// </summary>
    public bool HasChildren => _childList.Count > 0;

    /// <summary>
    /// The entities parented to this entity.
    /// </summary>
    public IReadOnlyList<Entity> Children => _childList;


    #region Creation and destruction

    public Entity(string? name = null) : this(null, name)
    {
        Application.Logger.Warn($"Creating an entity '{Name}' without a scene reference. This entity needs to be manually spawned in a scene.");
    }


    /// <summary>
    /// Creates a new entity with the given name.
    /// </summary>
    internal Entity(Scene? scene, string? name) : base(name)
    {
        SetScene(scene);
    }
    
    
    public void Spawn(Scene scene)
    {
        if (IsSpawned)
            throw new InvalidOperationException($"Entity {InstanceID} is already spawned in a scene.");

        SetScene(scene);
    }
    
    
    /// <summary>
    /// Changes the scene of this entity and all of its children.
    /// </summary>
    /// <param name="scene">The new scene to move the entity to. Null to remove the entity from the current scene.</param>
    internal void SetScene(Scene? scene)
    {
        if (Scene == scene)
            return;
        
        SetParent(null);
        
        PropagateSceneChange(scene);
    }
    
    
    private void PropagateSceneChange(Scene? scene)
    {
        Scene? oldScene = Scene;
        Scene = scene;
        _entityScene = scene?.EntityScene;

        oldScene?.EntityScene.UnregisterEntity(this);
        scene?.EntityScene.RegisterEntity(this);

        foreach (Entity child in _childList)
            child.PropagateSceneChange(scene);
    }


    protected override void OnDispose(bool manual)
    {
        Debug.Assert(manual, "Entity was not manually disposed of!");
        // We can safely do a while loop here because the recursive call to Destroy() will remove the child from the list.
        while (_childList.Count > 0)
            _childList[0].DestroyImmediate();

        RemoveAllSystems();

        foreach (EntityComponent component in _components)
            component.Destroy();

        _components.Clear();
        _componentCache.Clear();

        _entityScene?.UnregisterEntity(this);
        Parent?._childList.Remove(this);
    }

    #endregion
}
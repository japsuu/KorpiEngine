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
    private bool _isEnabledInHierarchy = true;
    private Scene? _scene;
    private EntityScene? _entityScene;
    private Entity? _parent;
    private readonly List<Entity> _childList = [];
    private readonly Transform _transform = new();
    private readonly List<EntityComponent> _components = [];
    private readonly MultiValueDictionary<Type, EntityComponent> _componentCache = new();
    private readonly Dictionary<ulong, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _systemBuckets = new();
    
    private bool IsParentEnabled => _parent == null || _parent._isEnabledInHierarchy;

    internal IReadOnlyCollection<EntityComponent> Components => _components;

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

    /// <summary>
    /// True if the entity is enabled and all of its parents are enabled, false otherwise.
    /// </summary>
    public bool IsEnabledInHierarchy => _isEnabledInHierarchy;

    /// <summary>
    /// True if this entity has no parent, false otherwise.
    /// </summary>
    public bool IsRootEntity => _parent == null;
    
    /// <summary>
    /// True if the entity is spawned in a scene, false otherwise.
    /// </summary>
    public bool IsSpawned => _scene != null;
    
    /// <summary>
    /// True if this entity has children, false otherwise.
    /// </summary>
    public bool HasChildren => _childList.Count > 0;
    
    /// <summary>
    /// The scene this entity is in.
    /// If null, the entity has not been spawned in a scene.
    /// </summary>
    public Scene? Scene => _scene;

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
    /// The hierarchical parent of this entity, or null if it is a root entity.
    /// </summary>
    public Entity? Parent => _parent;

    /// <summary>
    /// The entities parented to this entity.
    /// </summary>
    public IReadOnlyList<Entity> Children => _childList;

    public Matrix4x4 GlobalCameraRelativeTransform
    {
        get
        {
            Matrix4x4 t = Transform.LocalToWorldMatrix;
            return t.SetTranslation(t.Translation - Camera.RenderingCamera.Transform.Position);
        }
    }


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
        if (_scene == scene)
            return;
        
        SetParent(null);
        
        PropagateSceneChange(scene);
    }
    
    
    private void PropagateSceneChange(Scene? scene)
    {
        Scene? oldScene = _scene;
        _scene = scene;
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
        _parent?._childList.Remove(this);
    }

    #endregion
}
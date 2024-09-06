using System.Diagnostics;
using System.Reflection;
using KorpiEngine.EntityModel.IDs;
using KorpiEngine.EntityModel.SpatialHierarchy;
using KorpiEngine.Rendering.Cameras;
using KorpiEngine.SceneManagement;
using KorpiEngine.Utils;

namespace KorpiEngine.EntityModel;

/// <summary>
/// Container for components and systems.
/// </summary>
#warning TODO: Split Entity.cs to partial classes
public sealed class Entity : AssetInstance
{
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
    /// True if the entity is spawned in a scene, false otherwise.
    /// </summary>
    public bool IsSpawned => Scene != null;

    /// <summary>
    /// True if the entity is enabled explicitly, false otherwise.
    /// This value is unaffected by the entity's parent hierarchy.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value != _enabled)
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
    /// True if this entity has no parent, false otherwise.
    /// </summary>
    public bool IsRootEntity => Parent == null;
    
    /// <summary>
    /// True if this entity has children, false otherwise.
    /// </summary>
    public bool HasChildren => _childList.Count > 0;

    /// <summary>
    /// The entities parented to this entity.
    /// </summary>
    public IReadOnlyList<Entity> Children => _childList;

    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;
    internal IReadOnlyCollection<EntityComponent> Components => _components;

    private bool _enabled = true;
    private EntityScene? _entityScene;
    private readonly List<Entity> _childList = [];
    private readonly Transform _transform = new();
    private readonly List<EntityComponent> _components = [];
    private readonly MultiValueDictionary<Type, EntityComponent> _componentCache = new();
    private readonly Dictionary<ulong, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _systemBuckets = new();
    
    private bool IsParentEnabled => Parent == null || Parent.EnabledInHierarchy;


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
        Name = name ?? $"Entity {InstanceID}";
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


    #region Spatial Hierarchy

    /// <returns>True if <paramref name="testChild"/> is a child of <paramref name="testParent"/> or the same transform, false otherwise.</returns>
    public static bool IsChildOrSameTransform(Entity? testChild, Entity testParent)
    {
        Entity? child = testChild;
        while (child != null)
        {
            if (child == testParent)
                return true;
            child = child.Parent;
        }

        return false;
    }


    public bool IsChildOf(Entity testParent)
    {
        if (InstanceID == testParent.InstanceID)
            return false;

        return IsChildOrSameTransform(this, testParent);
    }


    public bool SetParent(Entity? newParent, bool worldPositionStays = true)
    {
        if (newParent == Parent)
            return true;

        // Make sure that the new father is not a child of this transform.
        if (IsChildOrSameTransform(newParent, this))
            return false;

        // Save the old position in world space
        Vector3 worldPosition = new();
        Quaternion worldRotation = new();
        Matrix4x4 worldScale = new();

        if (worldPositionStays)
        {
            worldPosition = Transform.Position;
            worldRotation = Transform.Rotation;
            worldScale = Transform.GetWorldRotationAndScale();
        }

        if (newParent != Parent)
        {
            Parent?._childList.Remove(this);

            if (newParent != null)
                newParent._childList.Add(this);

            Parent = newParent;
        }

        if (worldPositionStays)
        {
            if (Parent != null)
            {
                Transform.LocalPosition = Parent.Transform.InverseTransformPoint(worldPosition);
                Transform.LocalRotation = (Parent.Transform.Rotation.Inverse() * worldRotation).NormalizeSafe();
            }
            else
            {
                Transform.LocalPosition = worldPosition;
                Transform.LocalRotation = worldRotation.NormalizeSafe();
            }

            Transform.LocalScale = Vector3.One;
            Matrix4x4 inverseRotationScale = Transform.GetWorldRotationAndScale().Inverse() * worldScale;
            Transform.LocalScale = new Vector3(inverseRotationScale.M11, inverseRotationScale.M22, inverseRotationScale.M33);
        }

        HierarchyStateChanged();

        return true;
    }


    private void HierarchyStateChanged()
    {
        bool newState = _enabled && IsParentEnabled;
        if (EnabledInHierarchy != newState)
        {
            EnabledInHierarchy = newState;
            foreach (EntityComponent component in GetComponents<EntityComponent>())
                component.HierarchyStateChanged();
        }

        foreach (Entity child in _childList)
            child.HierarchyStateChanged();
    }


    private void SetEnabled(bool state)
    {
        _enabled = state;
        HierarchyStateChanged();
    }

    #endregion


    #region Component API

    #region AddComponent API

    public T AddComponent<T>() where T : EntityComponent, new()
    {
        if (IsDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        Type type = typeof(T);
        return (AddComponent(type) as T)!;
    }


    private EntityComponent AddComponent(Type type)
    {
        if (!typeof(EntityComponent).IsAssignableFrom(type))
            throw new InvalidOperationException($"The type {type.Name} does not inherit from EntityComponent");

        RequireComponentAttribute? requireComponentAttribute = type.GetCustomAttribute<RequireComponentAttribute>();
        if (requireComponentAttribute != null)
            foreach (Type requiredComponentType in requireComponentAttribute.Types)
            {
                if (!typeof(EntityComponent).IsAssignableFrom(requiredComponentType))
                    continue;

                // If there is already a component on the object
                if (GetComponent(requiredComponentType) != null)
                    continue;

                // Recursive call to attempt to add the new component
                AddComponent(requiredComponentType);
            }

        bool disallowMultiple = type.GetCustomAttribute<DisallowMultipleComponentAttribute>() != null;
        bool hasComponent = GetComponent(type) != null;

        if (disallowMultiple && hasComponent)
            throw new InvalidOperationException(
                $"Can't add the same component multiple times: the component of type {type.Name} does not allow multiple instances");

        if (Activator.CreateInstance(type) is not EntityComponent comp)
            throw new InvalidOperationException($"Failed to create component of type {type.Name}");

        comp.Bind(this);
        _components.Add(comp);
        _componentCache.Add(type, comp);

        if (EnabledInHierarchy)
            comp.InternalAwake();

        RegisterComponentWithSystems(comp);

        return comp;
    }

    #endregion


    #region RemoveComponent API

    public void RemoveAll<T>() where T : EntityComponent //TODO: Accepting interfaces
    {
        if (!_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
            return;

        foreach (EntityComponent component in components)
        {
            component.Destroy();

            _components.Remove(component);
        }

        _componentCache.Remove(typeof(T));

        foreach (EntityComponent component in components)
            UnregisterComponentWithSystems(component);
    }


    public void RemoveComponent<T>(T component) where T : EntityComponent //TODO: Accepting interfaces
    {
        Type type = typeof(T);

        RemoveComponent(component, type);
    }


    public void RemoveComponent(EntityComponent component) //TODO: Accepting interfaces
    {
        Type type = component.GetType();

        RemoveComponent(component, type);
    }


    private void RemoveComponent<T>(T component, Type type) where T : EntityComponent
    {
        if (!component.CanBeDestroyed())
            return;

        _components.Remove(component);
        _componentCache.Remove(type, component);

        component.Destroy();

        UnregisterComponentWithSystems(component);
    }

    #endregion


    #region GetComponent API

    public T? GetComponent<T>() where T : EntityComponent => (T?)GetComponent(typeof(T));


    public EntityComponent? GetComponent(Type type)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (type == null)
            return null;

        if (_componentCache.TryGetValue(type, out IReadOnlyCollection<EntityComponent> components))
            return components.First();

        foreach (EntityComponent comp in _components)
            if (comp.GetType().IsAssignableTo(type))
                return comp;

        return null;
    }


    public IEnumerable<EntityComponent> GetAllComponents() => _components;

    public bool TryGetComponent<T>(out T? component) where T : EntityComponent => (component = GetComponent<T>()) != null;


    public IEnumerable<T> GetComponents<T>() where T : EntityComponent
    {
        if (_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
        {
            foreach (EntityComponent comp in components)
                if (comp.GetType().IsAssignableTo(typeof(T)))
                    yield return (T)comp;
        }
        else
        {
            foreach (KeyValuePair<Type, IReadOnlyCollection<EntityComponent>> kvp in _componentCache.ToArray())
                if (kvp.Key.GetTypeInfo().IsAssignableTo(typeof(T)))
                    foreach (EntityComponent comp in kvp.Value.ToArray())
                        yield return (T)comp;
        }
    }


    public T? GetComponentInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent =>
        (T?)GetComponentInParent(typeof(T), includeSelf, includeInactive);


    public EntityComponent? GetComponentInParent(Type type, bool includeSelf = true, bool includeInactive = false)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (type == null)
            return null;

        // First check the current Object
        EntityComponent? component;
        if (includeSelf && EnabledInHierarchy)
        {
            component = GetComponent(type);
            if (component != null)
                return component;
        }

        // Now check all parents
        Entity? parent = this;
        while ((parent = parent.Parent) != null)
            if (parent.EnabledInHierarchy || includeInactive)
            {
                component = parent.GetComponent(type);
                if (component != null)
                    return component;
            }

        return null;
    }


    public IEnumerable<T> GetComponentsInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && EnabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;

        // Now check all parents
        Entity? parent = this;
        while ((parent = parent.Parent) != null)
            if (parent.EnabledInHierarchy || includeInactive)
                foreach (T component in parent.GetComponents<T>())
                    yield return component;
    }


    public T? GetComponentInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent =>
        (T?)GetComponentInChildren(typeof(T), includeSelf, includeInactive);


    public EntityComponent? GetComponentInChildren(Type componentType, bool includeSelf = true, bool includeInactive = false)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (componentType == null)
            return null;

        // First check the current Object
        EntityComponent? component;
        if (includeSelf && EnabledInHierarchy)
        {
            component = GetComponent(componentType);
            if (component != null)
                return component;
        }

        // Now check all children
        foreach (Entity child in _childList)
            if (EnabledInHierarchy || includeInactive)
            {
                component = child.GetComponent(componentType) ?? child.GetComponentInChildren(componentType);
                if (component != null)
                    return component;
            }

        return null;
    }


    public IEnumerable<T> GetComponentsInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && EnabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;

        // Now check all children
        foreach (Entity child in _childList)
            if (EnabledInHierarchy || includeInactive)
                foreach (T component in child.GetComponentsInChildren<T>())
                    yield return component;
    }

    #endregion


    internal bool IsComponentRequired(EntityComponent requiredComponent, out Type dependentType)
    {
        Type componentType = requiredComponent.GetType();
        foreach (EntityComponent component in _components)
        {
            RequireComponentAttribute? requireComponentAttribute =
                component.GetType().GetCustomAttribute<RequireComponentAttribute>();
            if (requireComponentAttribute == null)
                continue;

            if (Array.TrueForAll(requireComponentAttribute.Types, type => type != componentType))
                continue;

            dependentType = component.GetType();
            return true;
        }

        dependentType = null!;
        return false;
    }

    #endregion


    #region Systems API

    public void AddSystem<T>() where T : class, IEntitySystem, new()
    {
        if (IsDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        T system = new();
        ulong typeId = TypeID.Get<T>();

        if (system.IsSingleton && _systems.ContainsKey(typeId))
            throw new InvalidOperationException($"Entity {InstanceID} already has a singleton system of type {typeof(T).Name}.");

        if (system.UpdateStages.Length <= 0)
            throw new InvalidOperationException($"System of type {typeof(T).Name} does not specify when it should be updated.");

        _systems.Add(typeId, system);

        _systemBuckets.AddSystem(typeId, system);

        system.OnRegister(this);
    }


    public void RemoveSystem<T>() where T : class, IEntitySystem
    {
        if (IsDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        ulong typeId = TypeID.Get<T>();

        if (!_systems.Remove(typeId, out IEntitySystem? system))
            throw new InvalidOperationException($"Entity {InstanceID} does not have a system of type {typeof(T).Name}.");

        _systemBuckets.RemoveSystem(typeId);

        system.OnUnregister(this);
    }


    private void RemoveAllSystems()
    {
        foreach (IEntitySystem system in _systems.Values)
            system.OnUnregister(this);

        _systems.Clear();
        _systemBuckets.Clear();
    }


    private void RegisterComponentWithSystems(EntityComponent component)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryRegisterComponent(component);

        _entityScene?.RegisterComponent(component);
    }


    private void UnregisterComponentWithSystems(EntityComponent component)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);

        _entityScene?.UnregisterComponent(component);
    }

    #endregion


    #region Updating

    internal void EnsureComponentInitialization()
    {
        foreach (EntityComponent component in _components)
        {
            if (!component.HasAwoken)
                component.InternalAwake();

            if (component.HasStarted)
                continue;

            if (component.EnabledInHierarchy)
                component.InternalStart();
        }
    }
    
    
    internal void Update(EntityUpdateStage stage)
    {
        UpdateComponentsRecursive(stage);
        UpdateSystemsRecursive(stage);
    }


    /// <summary>
    /// Propagates system updates downwards in the hierarchy.
    /// </summary>
    private void UpdateSystemsRecursive(EntityUpdateStage stage)
    {
        _systemBuckets.Update(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _childList)
            child.UpdateSystemsRecursive(stage);
    }


    /// <summary>
    /// Propagates component updates downwards in the hierarchy.
    /// </summary>
    private void UpdateComponentsRecursive(EntityUpdateStage stage)
    {
        foreach (EntityComponent component in _components)
            component.Update(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _childList)
            child.UpdateComponentsRecursive(stage);
    }

    #endregion
}
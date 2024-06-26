using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Utils;
using System.Reflection;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Container for components and systems that make up an entity.
/// </summary>
public sealed class Entity //TODO: Split to partial classes
{
    public readonly ulong InstanceID;
    public readonly Scene Scene;

    /// <summary>
    /// The name of this entity.
    /// </summary>
    public string Name;

    public Transform Transform
    {
        get
        {
            _transform.Entity = this;
            return _transform;
        }
    }

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

    /// <summary>
    /// True if the entity is enabled and all of its parents are enabled, false otherwise.
    /// </summary>
    public bool EnabledInHierarchy => _enabledInHierarchy;

    public bool IsRootEntity => _parent == null;
    public bool HasChildren => _children.Count > 0;
    public Entity? Parent => _parent;
    public IReadOnlyList<Entity> Children => _children;

    internal bool IsDestroyed => _isDestroyed;
    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;
    internal List<Entity> ChildList => _children;

    private bool _enabled = true;
    private bool _enabledInHierarchy = true;
    private bool IsParentEnabled => _parent == null || _parent._enabledInHierarchy;
    private bool _isDestroyed;
    private Entity? _parent;
    private readonly List<Entity> _children = [];
    private readonly Transform _transform = new();
    private readonly EntityScene _entityScene;
    private readonly List<EntityComponent> _components = [];
    private readonly MultiValueDictionary<Type, EntityComponent> _componentCache = new();
    private readonly Dictionary<ulong, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _systemBuckets = new();


    #region Creation and destruction

    public Entity(string? name = null) : this(SceneManager.CurrentScene, name)
    {
    }


    /// <summary>
    /// Creates a new entity with the given name.
    /// </summary>
    internal Entity(Scene scene, string? name)
    {
        InstanceID = EntityID.Generate();
        Name = name ?? $"Entity {InstanceID}";
        Scene = scene;
        _entityScene = scene.EntityScene;

        _entityScene.RegisterEntity(this);
    }


    ~Entity()
    {
        if (_isDestroyed)
            return;

        Application.Logger.Warn($"Entity {InstanceID} ({Name}) was not destroyed before being garbage collected. This is a memory leak.");
        Destroy();
    }


    /// <summary>
    /// Destroys the entity and all of its components and systems.
    /// </summary>
    public void Destroy()
    {
        while (_children.Count > 0)
            _children[0].Destroy();

        RemoveAllSystems();

        foreach (EntityComponent component in _components)
        {
            if (component.EnabledInHierarchy)
                component.OnDisable();

            if (component.HasStarted)
                component.OnDestroy(); // OnDestroy is only called if the component has previously been active

            component.Dispose();
        }

        _components.Clear();
        _componentCache.Clear();

        _entityScene.UnregisterEntity(this);

        _isDestroyed = true;
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
            child = child._parent;
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
        if (newParent == _parent)
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

        if (newParent != _parent)
        {
            _parent?._children.Remove(this);

            if (newParent != null)
                newParent._children.Add(this);

            _parent = newParent;
        }

        if (worldPositionStays)
        {
            if (_parent != null)
            {
                Transform.LocalPosition = _parent.Transform.InverseTransformPoint(worldPosition);
                Transform.LocalRotation = Quaternion.NormalizeSafe(Quaternion.Inverse(_parent.Transform.Rotation) * worldRotation);
            }
            else
            {
                Transform.LocalPosition = worldPosition;
                Transform.LocalRotation = Quaternion.NormalizeSafe(worldRotation);
            }

            Transform.LocalScale = Vector3.One;
            Matrix4x4 inverseRotationScale = Transform.GetWorldRotationAndScale().Invert() * worldScale;
            Transform.LocalScale = new Vector3(inverseRotationScale[0, 0], inverseRotationScale[1, 1], inverseRotationScale[2, 2]);
        }

        HierarchyStateChanged();

        return true;
    }


    private void HierarchyStateChanged()
    {
        bool newState = _enabled && IsParentEnabled;
        if (_enabledInHierarchy != newState)
        {
            _enabledInHierarchy = newState;
            foreach (EntityComponent component in GetComponents<EntityComponent>())
                component.HierarchyStateChanged();
        }

        foreach (Entity child in _children)
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
        if (_isDestroyed)
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

        if (_enabledInHierarchy)
            comp.InternalAwake();

        RegisterComponentWithSystems(comp);

        return comp;
    }


    /*public void AddComponent(EntityComponent comp)
    {
        Type type = comp.GetType();
        RequireComponentAttribute? requireComponentAttribute = type.GetCustomAttribute<RequireComponentAttribute>();
        if (requireComponentAttribute != null)
        {
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
        }

        bool disallowMultiple = type.GetCustomAttribute<DisallowMultipleComponentAttribute>() != null;
        bool hasComponent = GetComponent(type) != null;

        if (disallowMultiple && hasComponent)
            throw new InvalidOperationException(
                $"Can't add the same component multiple times: the component of type {type.Name} does not allow multiple instances");

        comp.Bind(this);

        _components.Add(comp);
        _componentCache.Add(comp.GetType(), comp);

        if (EnabledInHierarchy)
            comp.InternalAwake();
    }*/

    #endregion


    #region RemoveComponent API

    public void RemoveAll<T>() where T : EntityComponent //TODO: Accepting interfaces
    {
        if (!_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
            return;

        foreach (EntityComponent c in components)
            if (c.EnabledInHierarchy)
                c.OnDisable();

        foreach (EntityComponent c in components)
        {
            // OnDestroy is only called if the component has previously been active
            if (c.HasStarted)
                c.OnDestroy();

            _components.Remove(c);
        }

        _componentCache.Remove(typeof(T));

        foreach (EntityComponent c in components)
            UnregisterComponentWithSystems(c);
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

        if (component.EnabledInHierarchy)
            component.OnDisable();

        // OnDestroy is only called if the component has previously been active
        if (component.HasStarted)
            component.OnDestroy();

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
        if (!_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
        {
            foreach (KeyValuePair<Type, IReadOnlyCollection<EntityComponent>> kvp in _componentCache.ToArray())
                if (kvp.Key.GetTypeInfo().IsAssignableTo(typeof(T)))
                    foreach (EntityComponent comp in kvp.Value.ToArray())
                        yield return (T)comp;
        }
        else
        {
            foreach (EntityComponent comp in components)
                if (comp.GetType().IsAssignableTo(typeof(T)))
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
        foreach (Entity child in _children)
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
        foreach (Entity child in _children)
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

            if (requireComponentAttribute.Types.All(type => type != componentType))
                continue;

            dependentType = component.GetType();
            return true;
        }

        dependentType = null!;
        return false;
    }

    #endregion


    #region Systems API

    public void AddSystem<T>() where T : IEntitySystem, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        T system = new();
        ulong id = EntitySystemID.Generate<T>();

        if (system.IsSingleton)
            if (_systems.ContainsKey(id))
                throw new InvalidOperationException($"Entity {InstanceID} already has a singleton system of type {typeof(T).Name}.");

        if (system.UpdateStages.Length <= 0)
            throw new InvalidOperationException($"System of type {typeof(T).Name} does not specify when it should be updated.");

        _systems.Add(id, system);

        _systemBuckets.AddSystem(id, system);

        system.OnRegister(this);
    }


    public void RemoveSystem<T>() where T : IEntitySystem
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        ulong id = EntitySystemID.Generate<T>();

        if (!_systems.Remove(id, out IEntitySystem? system))
            throw new InvalidOperationException($"Entity {InstanceID} does not have a system of type {typeof(T).Name}.");

        _systemBuckets.RemoveSystem(id);

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

        _entityScene.RegisterComponent(component);
    }


    private void UnregisterComponentWithSystems(EntityComponent component)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);

        _entityScene.UnregisterComponent(component);
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


    /// <summary>
    /// Propagates system updates downwards in the hierarchy.
    /// </summary>
    internal void UpdateSystemsRecursive(EntityUpdateStage stage)
    {
        _systemBuckets.Update(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _children)
            child.UpdateSystemsRecursive(stage);
    }


    /// <summary>
    /// Propagates component updates downwards in the hierarchy.
    /// </summary>
    internal void UpdateComponentsRecursive(EntityUpdateStage stage)
    {
        InvokeComponentUpdates(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _children)
            child.UpdateComponentsRecursive(stage);
    }


    private void InvokeComponentUpdates(EntityUpdateStage stage)
    {
        foreach (EntityComponent component in _components)
            component.Update(stage);
    }

    #endregion
}
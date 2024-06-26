﻿using System.Diagnostics.Contracts;
using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Utils;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Container for components and systems that make up an entity.
/// </summary>
public sealed class Entity
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

    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;

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
    private readonly Dictionary<EntitySystemID, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _buckets = new();


    private void SetEnabled(bool state)
    {
        _enabled = state;
        HierarchyStateChanged();
    }


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
        _entityScene.UnregisterEntity(this);

        RemoveAllSystems();

        RemoveAllComponents();

        if (IsSpatial)
            RemoveFromSpatialHierarchy();

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

    #endregion


    #region Adding and removing components
    
    
    
    
    
    
    
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
            throw new InvalidOperationException($"Can't add the same component multiple times: the component of type {type.Name} does not allow multiple instances");

        if (Activator.CreateInstance(type) is not EntityComponent newComponent)
            throw new InvalidOperationException($"Failed to create component of type {type.Name}");

        newComponent.Bind(this);
        _components.Add(newComponent);
        _componentCache.Add(type, newComponent);

        if (_enabledInHierarchy)
        {
            newComponent.InternalAwake();
        }

        return newComponent;
    }

    public void AddComponent(EntityComponent comp)
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

        if (type.GetCustomAttribute<DisallowMultipleComponentAttribute>() != null && GetComponent(type) != null)
        {
            Application.Logger.Error($"Can't Add the Same Component Multiple TimesThe component of type {type.Name} does not allow multiple instances");
            return;
        }

        comp.AttachToGameObject(this);
        _components.Add(comp);
        _componentCache.Add(comp.GetType(), comp);
        if (enabledInHierarchy)
        {
            comp.Do(comp.InternalAwake);
        }
    }

    public void RemoveAll<T>() where T : EntityComponent
    {
        IReadOnlyCollection<EntityComponent> components;
        if (_componentCache.TryGetValue(typeof(T), out components))
        {
            foreach (EntityComponent c in components)
                if (c.EnabledInHierarchy)
                    c.Do(c.OnDisable);
            foreach (EntityComponent c in components)
            {
                if (c.HasStarted) // OnDestroy is only called if the component has previously been active
                    c.Do(c.OnDestroy);

                _components.Remove(c);
            }
            _componentCache.Remove(typeof(T));
        }
    }

    public void RemoveComponent<T>(T component) where T : EntityComponent
    {
        if (component.CanDestroy() == false) return;

        _components.Remove(component);
        _componentCache.Remove(typeof(T), component);

        if (component.EnabledInHierarchy) component.Do(component.OnDisable);
        if (component.HasStarted) component.Do(component.OnDestroy); // OnDestroy is only called if the component has previously been active
    }

    public void RemoveComponent(EntityComponent component)
    {
        if (component.CanDestroy() == false) return;

        _components.Remove(component);
        _componentCache.Remove(component.GetType(), component);

        if (component.EnabledInHierarchy) component.Do(component.OnDisable);
        if (component.HasStarted) component.Do(component.OnDestroy); // OnDestroy is only called if the component has previously been active
    }

    public T? GetComponent<T>() where T : EntityComponent => (T?)GetComponent(typeof(T));

    public EntityComponent? GetComponent(Type type)
    {
        if (type == null) return null; 
        if (_componentCache.TryGetValue(type, out IReadOnlyCollection<EntityComponent> components))
            return components.First();
        else
            foreach (EntityComponent comp in _components)
                if (comp.GetType().IsAssignableTo(type))
                    return comp;
        return null;
    }

    public IEnumerable<EntityComponent> GetComponents() => _components;

    public bool TryGetComponent<T>(out T? component) where T : EntityComponent => (component = GetComponent<T>()) != null;

    public IEnumerable<T> GetComponents<T>() where T : EntityComponent
    {
        if (typeof(T) == typeof(EntityComponent))
        {
            // Special case for Component
            foreach (EntityComponent comp in _components)
                yield return (T)comp;
        }
        else
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
    }

    public T? GetComponentInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent => (T)GetComponentInParent(typeof(T), includeSelf, includeInactive);

    public EntityComponent? GetComponentInParent(Type componentType, bool includeSelf = true, bool includeInactive = false)
    {
        if (componentType == null) return null;
        // First check the current Object
        EntityComponent component;
        if (includeSelf && enabledInHierarchy) {
            component = GetComponent(componentType);
            if (component != null)
                return component;
        }
        // Now check all parents
        GameObject parent = this;
        while ((parent = parent.parent) != null)
        {
            if (parent.enabledInHierarchy || includeInactive)
            {
                component = parent.GetComponent(componentType);
                if (component != null)
                    return component;
            }
        }
        return null;
    }

    public IEnumerable<T> GetComponentsInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && enabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;
        // Now check all parents
        GameObject parent = this;
        while ((parent = parent.parent) != null) {
            if(parent.enabledInHierarchy || includeInactive)
                foreach (var component in parent.GetComponents<T>())
                    yield return component;
        }
    }

    public T? GetComponentInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent => (T)GetComponentInChildren(typeof(T), includeSelf, includeInactive);

    public EntityComponent GetComponentInChildren(Type componentType, bool includeSelf = true, bool includeInactive = false)
    {
        if (componentType == null) return null;
        // First check the current Object
        EntityComponent component;
        if (includeSelf && enabledInHierarchy) {
            component = GetComponent(componentType);
            if (component != null)
                return component;
        }
        // Now check all children
        foreach (var child in children)
        {
            if (enabledInHierarchy || includeInactive)
            {
                component = child.GetComponent(componentType) ?? child.GetComponentInChildren(componentType);
                if (component != null)
                    return component;
            }
        }
        return null;
    }


    public IEnumerable<T> GetComponentsInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && enabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;
        // Now check all children
        foreach (var child in children)
        {
            if (enabledInHierarchy || includeInactive)
                foreach (var component in child.GetComponentsInChildren<T>())
                    yield return component;
        }
    }

    
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
    
    
    
    
    

    /*/// <summary>
    /// Adds a new component of the given type to the entity.
    /// </summary>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    public T AddComponent<T>() where T : EntityComponent, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        T component = new();
        component.Bind(this);

        return AddComponent(component);
    }


    private T AddComponent<T>(T component) where T : EntityComponent, new()
    {
        _components.Add(component);
        _componentCache.Add(component.GetType(), component);

        RegisterComponent(component);

        return component;
    }


    public void RemoveComponents<T>() where T : EntityComponent
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        foreach (T component in GetComponents<T>())
            RemoveComponent(component);
    }


    private void RemoveComponent<T>(T component) where T : EntityComponent
    {
        if (component is SpatialEntityComponent spatialComponent)
        {
            if (RootSpatialComponent != spatialComponent)
                spatialComponent.OnDestroy();

            RootSpatialComponent = null;
        }

        if (!_components.Remove(component))
            throw new InvalidOperationException($"Entity {InstanceID} does not have a component of type {typeof(T).Name}.");

        UnregisterComponent(component);
    }*/


    private void RegisterComponent<T>(T component) where T : EntityComponent, new()
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryRegisterComponent(component);

        _entityScene.RegisterComponent(component);

        component.OnRegister();
    }


    private void UnregisterComponent<T>(T component) where T : EntityComponent
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);

        _entityScene.UnregisterComponent(component);

        component.OnUnregister();
    }


    /*private void RemoveAllComponents()
    {
        foreach (EntityComponent component in _components.ToArray())
            RemoveComponent(component);
    }


    [Pure]
    internal IEnumerable<T> GetComponents<T>()
    {
        List<T> components = [];
        foreach (EntityComponent component in _components)
            if (component is T typedComponent)
                components.Add(typedComponent);

        return components;
    }*/

    #endregion


    #region Adding and removing systems

    public void AddSystem<T>() where T : IEntitySystem, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        T system = new();
        EntitySystemID id = EntitySystemID.Generate<T>();

        if (system.IsSingleton)
            if (_systems.ContainsKey(id))
                throw new InvalidOperationException($"Entity {InstanceID} already has a singleton system of type {typeof(T).Name}.");

        if (system.UpdateStages.Length <= 0)
            throw new InvalidOperationException($"System of type {typeof(T).Name} does not specify when it should be updated.");

        _systems.Add(id, system);

        _buckets.AddSystem(id, system);

        system.OnRegister(this);
    }


    public void RemoveSystem<T>() where T : IEntitySystem
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        EntitySystemID id = EntitySystemID.Generate<T>();

        if (!_systems.Remove(id, out IEntitySystem? system))
            throw new InvalidOperationException($"Entity {InstanceID} does not have a system of type {typeof(T).Name}.");

        _buckets.RemoveSystem(id);

        system.OnUnregister(this);
    }


    private void RemoveAllSystems()
    {
        foreach (IEntitySystem system in _systems.Values)
            system.OnUnregister(this);

        _systems.Clear();
        _buckets.Clear();
    }

    #endregion


    #region Updating

    internal void UpdateRecursive(SystemUpdateStage stage)
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        if (!_enabled)
            return;

        _buckets.Update(stage);

        if (!HasSpatialChildren)
            return;

        foreach (Entity child in _children)
            child.UpdateRecursive(stage);
    }

    #endregion
}
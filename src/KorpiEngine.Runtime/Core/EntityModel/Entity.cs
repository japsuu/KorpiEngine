using System.Diagnostics;
using System.Diagnostics.Contracts;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Container for components and systems that make up an entity.
/// May also contain a spatial hierarchy.
/// </summary>
public sealed class Entity
{
    public readonly EntityID ID;
    public readonly string Name;
    
    /// <summary>
    /// True, if the entity is enabled explicitly.
    /// This value is unaffected by the entity's parent hierarchy.
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    public bool IsEnabledInHierarchy { get; private set; }
    public bool IsSpatial { get; private set; }
    public bool IsSpatialRootEntity => _parent == null;
    public bool HasSpatialChildren => _children.Count > 0;

    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;
    internal readonly Scene Scene;
    internal readonly EntityScene EntityScene;
    
    private bool IsParentEnabled => _parent == null || _parent.IsEnabledInHierarchy;
    private bool _isDestroyed;
    private Entity? _parent;
    private readonly List<Entity> _children = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<EntitySystemID, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _buckets = new();
    private SpatialEntityComponent? _rootSpatialComponent;
    public SpatialEntityComponent? RootSpatialComponent
    {
        get => _rootSpatialComponent;
        set
        {
            _rootSpatialComponent = value;
            IsSpatial = value != null;
        }
    }
    
    
    public void SetEnabled(bool enabled)
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");
        
        if (IsEnabled == enabled)
            return;
        
        IsEnabled = enabled;
        
        // foreach (IEntitySystem system in _systems.Values)
        //     system.OnEntityEnabledChanged(this, enabled);
    }


    #region Creation and destruction

    /// <summary>
    /// Creates a new entity with the given name.
    /// </summary>
    internal Entity(Scene scene, string? name, string? spatialSocketID = null)
    {
        ID = EntityID.Generate();
        Name = name ?? $"Entity {ID}";
        Scene = scene;
        EntityScene = scene.EntityScene;
        
        EntityScene.RegisterEntity(this);

        if (string.IsNullOrWhiteSpace(spatialSocketID))
            return;
        
        AddComponent<SpatialEntityComponent>();
        RootSpatialComponent!.SocketID = spatialSocketID;
    }
    
    
    ~Entity()
    {
        if (_isDestroyed)
            return;
        
        Application.Logger.Warn($"Entity {ID} ({Name}) was not destroyed before being garbage collected. This is a memory leak.");
        Destroy();
    }
    
    
    /// <summary>
    /// Destroys the entity and all of its components and systems.
    /// </summary>
    public void Destroy()
    {
        EntityScene.UnregisterEntity(this);
        
        RemoveAllSystems();
        
        RemoveAllComponents();
        
        if (IsSpatial)
            RemoveFromSpatialHierarchy();
        
        _isDestroyed = true;
    }


    private void RemoveFromSpatialHierarchy()
    {
        foreach (Entity child in _children)
            child.ClearParent();
        
        ClearParent();
    }

    #endregion


    #region Spatial Hierarchy
    
    public void ClearParent()
    {
        SetParent(null);
    }
    

    public void SetParent(Entity? newParent, string? targetSpatialSocketID = null)
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");
        
        if (RootSpatialComponent == null)
            throw new InvalidOperationException("Cannot set a parent for an entity without a spatial component.");
        
        if (newParent != null && newParent.RootSpatialComponent == null)
            throw new InvalidOperationException("Cannot set a non-spatial entity as a parent.");
        
        if (newParent == this)
            throw new InvalidOperationException("Cannot set an entity as its own parent.");
        
        if (_parent == newParent)
            return;

        SpatialEntityComponent? targetSpatialComponent;
        
        // If new parent not specified, set the parent to null
        if (newParent == null)
        {
            targetSpatialComponent = null;
        }
        // If target socket id not specified, set the parent to the root spatial component
        else if (targetSpatialSocketID == null)
        {
            targetSpatialComponent = newParent.RootSpatialComponent;
        }
        // Otherwise, find the target spatial component and set the parent to that
        else
        {
            targetSpatialComponent = newParent.RootSpatialComponent!.FindSpatialComponentWithSocket(targetSpatialSocketID);
        
            if (targetSpatialComponent == null)
                throw new InvalidOperationException($"Could not find a spatial component with socket ID {targetSpatialSocketID}.");
        }
        
        RootSpatialComponent.SetParent(targetSpatialComponent);
        _parent?._children.Remove(this);
        _parent = newParent;
        _parent?._children.Add(this);
        
        HierarchyStateChanged();
    }


    private void HierarchyStateChanged()
    {
        bool newState = IsEnabled && IsParentEnabled;
        IsEnabledInHierarchy = newState;

        foreach (Entity child in _children)
            child.HierarchyStateChanged();
    }

    #endregion


    #region Adding and removing components

    /// <summary>
    /// Adds a new component of the given type to the entity.
    /// </summary>
    /// <param name="spatialSocketID">The SocketID to assign the component. Only required for spatial components.</param>
    /// <param name="targetSpatialSocketID">The SocketID of the other component to attach the added component to. Only required for spatial components.</param>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    public T AddComponent<T>(string? spatialSocketID = null, string? targetSpatialSocketID = null) where T : EntityComponent, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");

        T component = new();
        
        return AddComponent(component, spatialSocketID, targetSpatialSocketID);
    }


    public T AddComponent<T>(T component, string? spatialSocketID = null, string? targetSpatialSocketID = null) where T : EntityComponent, new()
    {
        if (component is SpatialEntityComponent spatialComponent)
        {
            if (string.IsNullOrWhiteSpace(spatialSocketID))
            {
                if (RootSpatialComponent == null)
                    spatialSocketID = "root";
                else
                    throw new InvalidOperationException("Spatial components require a socket ID.");
            }
            
            spatialComponent.SocketID = spatialSocketID;
            
            if (RootSpatialComponent == null)
            {
                if (targetSpatialSocketID != null)
                    throw new InvalidOperationException("Cannot attach a spatial component to a socket without a root spatial component.");
                
                RootSpatialComponent = spatialComponent;
            }
            else
            {
                SpatialEntityComponent? targetSpatialComponent = targetSpatialSocketID != null
                    ? RootSpatialComponent.FindSpatialComponentWithSocket(targetSpatialSocketID)
                    : RootSpatialComponent;
                
                if (targetSpatialComponent == null)
                    throw new InvalidOperationException($"Could not find a spatial component with socket ID {targetSpatialSocketID}.");
                
                spatialComponent.SetParent(targetSpatialComponent);
            }
        }
        else if (spatialSocketID != null || targetSpatialSocketID != null)
            throw new InvalidOperationException($"Component of type {typeof(T).Name} does not support spatial sockets.");
        
        component.EntityID = ID;
        
        _components.Add(component);

        RegisterComponent(component);
        
        return component;
    }


    public void RemoveComponents<T>() where T : EntityComponent
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");

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
            throw new InvalidOperationException($"Entity {ID} does not have a component of type {typeof(T).Name}.");
            
        UnregisterComponent(component);
    }


    private void RegisterComponent<T>(T component) where T : EntityComponent, new()
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryRegisterComponent(component);
        
        EntityScene.RegisterComponent(component);
        
        component.OnRegister();
    }


    private void UnregisterComponent<T>(T component) where T : EntityComponent
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);
            
        EntityScene.UnregisterComponent(component);
            
        component.OnUnregister();
    }


    private void RemoveAllComponents()
    {
        foreach (EntityComponent component in _components.ToArray())
            RemoveComponent(component);
    }


    [Pure]
    internal List<T> GetComponents<T>() where T : EntityComponent
    {
        List<T> components = [];
        foreach (EntityComponent component in _components)
        {
            if (component is T typedComponent)
                components.Add(typedComponent);
        }
        
        // Debug.Assert(components.Count <= 0, $"Entity {ID} has no components of type {typeof(T).Name}.");
        
        return components;
    }

    #endregion


    #region Adding and removing systems

    public void AddSystem<T>() where T : IEntitySystem, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");
        
        T system = new();
        EntitySystemID id = EntitySystemID.Generate<T>();
        
        if (system.IsSingleton)
        {
            if (_systems.ContainsKey(id))
                throw new InvalidOperationException($"Entity {ID} already has a singleton system of type {typeof(T).Name}.");
        }
        
        if (system.UpdateStages.Length <= 0)
            throw new InvalidOperationException($"System of type {typeof(T).Name} does not specify when it should be updated.");
        
        _systems.Add(id, system);
        
        _buckets.AddSystem(id, system);
        
        system.OnRegister(this);
    }


    public void RemoveSystem<T>() where T : IEntitySystem
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");

        EntitySystemID id = EntitySystemID.Generate<T>();
        
        if (!_systems.Remove(id, out IEntitySystem? system))
            throw new InvalidOperationException($"Entity {ID} does not have a system of type {typeof(T).Name}.");

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
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");
        
        if (!IsEnabled)
            return;

        _buckets.Update(stage);

        if (!HasSpatialChildren)
            return;
        
        foreach (Entity child in _children)
            child.UpdateRecursive(stage);
    }

    #endregion
}
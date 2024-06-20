using System.Diagnostics;
using System.Diagnostics.Contracts;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Container for components and systems that make up an entity.
/// May also contain a spatial hierarchy.
/// </summary>
public sealed class Entity
{
    public readonly EntityID ID;
    public readonly string Name;
    
    internal SpatialEntityComponent? RootSpatialComponent { get; private set; }
    internal int ComponentCount => _components.Count;
    internal int SystemCount => _systems.Count;
    internal bool IsSpatial => RootSpatialComponent != null;
    
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<EntitySystemID, IEntitySystem> _systems = [];
    private readonly SystemBucketCollection _buckets = new();
    
    private bool _isDestroyed;


    #region Creation and destruction

    /// <summary>
    /// Creates a new entity with the given name.
    /// </summary>
    public Entity(string? name, bool isSpatial = false)
    {
        ID = EntityID.Generate();
        Name = name ?? $"Entity {ID}";
        EntityWorld.RegisterEntity(this);
        
        if (isSpatial)
            AddComponent<SpatialEntityComponent>();
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
        EntityWorld.UnregisterEntity(this);
        
        RemoveAllSystems();
        
        RemoveAllComponents();
        
        _isDestroyed = true;
    }

    #endregion


    #region Adding and removing components

    public void AddComponent<T>() where T : EntityComponent, new()
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");

        T component = new();
        
        if (component is SpatialEntityComponent spatialComponent)
        {
            if (RootSpatialComponent != null)
                throw new NotImplementedException("TODO: Spatial hierarchy.");
            
            RootSpatialComponent = spatialComponent;
        }
        
        _components.Add(component);

        RegisterComponent(component);
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
                throw new NotImplementedException("TODO: Spatial hierarchy.");
            
            RootSpatialComponent = null;
        }
        
        if (!_components.Remove(component))
            throw new InvalidOperationException($"Entity {ID} does not have a component of type {typeof(T).Name}.");
            
        UnregisterComponent(component);
    }


    private void RegisterComponent<T>(T component) where T : EntityComponent, new()
    {
        component.EntityID = ID;
        
        foreach (IEntitySystem system in _systems.Values)
            system.TryRegisterComponent(component);
        
        EntityWorld.RegisterComponent(component);
        
        component.OnRegister();
    }


    private void UnregisterComponent<T>(T component) where T : EntityComponent
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);
            
        EntityWorld.UnregisterComponent(component);
            
        component.OnUnregister();
    }


    private void RemoveAllComponents()
    {
        foreach (EntityComponent component in _components)
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
        
        Debug.Assert(components.Count <= 0, $"Entity {ID} has no components of type {typeof(T).Name}.");
        
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
    
    internal void Update(SystemUpdateStage stage)
    {
        if (_isDestroyed)
            throw new InvalidOperationException($"Entity {ID} has been destroyed.");

        _buckets.Update(stage);
    }

    #endregion
}
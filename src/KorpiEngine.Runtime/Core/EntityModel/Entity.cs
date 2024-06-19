using System.Diagnostics;
using System.Diagnostics.Contracts;
using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

public sealed class Entity
{
    public readonly EntityID ID;
    public readonly string Name;
    
    internal SpatialEntityComponent? RootSpatialComponent;
    
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<EntitySystemID, IEntitySystem> _systemsById = [];
    private readonly NestedDictionary<SystemUpdateStage, EntitySystemID, IEntitySystem> _systemsByUpdateStage = [];
    
    private bool _isDestroyed;


    #region Creation and destruction

    public Entity(string? name)
    {
        ID = EntityID.Generate();
        Name = name ?? $"Entity {ID}";
        EntityWorld.RegisterEntity(this);
    }
    
    
    ~Entity()
    {
        if (_isDestroyed)
            return;
        
        Application.Logger.Warn($"Entity {ID} ({Name}) was not destroyed before being garbage collected. This is a memory leak.");
        Destroy();
    }
    
    
    public void Destroy()
    {
        EntityWorld.UnregisterEntity(this);
        _isDestroyed = true;
    }

    #endregion


    #region Adding and removing components

    public void AddComponent<T>() where T : EntityComponent, new()
    {
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
        foreach (T component in GetComponents<T>())
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
    }


    private void RegisterComponent<T>(T component) where T : EntityComponent, new()
    {
        foreach (IEntitySystem system in _systemsById.Values)
            system.TryRegisterComponent(component);
        
        EntityWorld.RegisterComponent(component);
        
        component.OnRegister();
    }


    private void UnregisterComponent<T>(T component) where T : EntityComponent
    {
        foreach (IEntitySystem system in _systemsById.Values)
            system.TryUnregisterComponent(component);
            
        EntityWorld.UnregisterComponent(component);
            
        component.OnUnregister();
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
        T system = new();
        EntitySystemID id = EntitySystemID.Generate<T>();
        
        if (system.IsSingleton)
        {
            if (_systemsById.ContainsKey(id))
                throw new InvalidOperationException($"Entity {ID} already has a singleton system of type {typeof(T).Name}.");
        }
        
        if (system.UpdateStages.Length <= 0)
            throw new InvalidOperationException($"System of type {typeof(T).Name} does not specify when it should be updated.");
        
        _systemsById.Add(id, system);
        
        foreach (SystemUpdateStage stage in system.UpdateStages)
            _systemsByUpdateStage.Add(stage, id, system);
        
        system.OnRegister(this);
    }


    public void RemoveSystem<T>() where T : IEntitySystem
    {
        EntitySystemID id = EntitySystemID.Generate<T>();
        
        if (!_systemsById.Remove(id, out IEntitySystem? system))
            throw new InvalidOperationException($"Entity {ID} does not have a system of type {typeof(T).Name}.");

        foreach (SystemUpdateStage stage in system.UpdateStages)
            _systemsByUpdateStage.Remove(stage, id);
        
        system.OnUnregister(this);
    }

    #endregion
    
    
    #region Updating
    
    internal void Update(SystemUpdateStage stage)
    {
        foreach (IEntitySystem system in _systemsByUpdateStage.IterateValues(stage))
            system.Update(stage);
    }

    #endregion
}
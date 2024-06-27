using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Manages all entities, components, and systems in a scene.
/// </summary>
internal sealed class EntityScene
{
    private bool _isBeingDestroyed;
    private readonly List<Entity> _entities = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<SceneSystemID, SceneSystem> _sceneSystems = [];


    internal void RegisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;
        
        if (entity.ComponentCount > 0)
            throw new InvalidOperationException($"Entity {entity} has components before being registered. This is not allowed.");
        
        if (entity.SystemCount > 0)
            throw new InvalidOperationException($"Entity {entity} has systems before being registered. This is not allowed.");
        
        _entities.Add(entity);
    }


    internal void UnregisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;

        _entities.Remove(entity);
    }


    internal void RegisterComponent(EntityComponent component)
    {
        if (_isBeingDestroyed)
            return;

        _components.Add(component);
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryRegisterComponent(component);
    }


    internal void UnregisterComponent(EntityComponent component)
    {
        if (_isBeingDestroyed)
            return;

        if (!_components.Remove(component))
            throw new InvalidOperationException($"Component {component} is not registered.");
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryUnregisterComponent(component);
    }


    internal void RegisterSceneSystem<T>() where T : SceneSystem, new()
    {
        if (_isBeingDestroyed)
            return;

        SceneSystem system = new T();
        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.TryAdd(id, system))
            throw new InvalidOperationException($"Scene system {id} already exists. Scene systems are singletons by default.");
        
        system.OnRegister(_components);
    }
    
    
    internal void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        if (_isBeingDestroyed)
            return;

        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.Remove(id, out SceneSystem? system))
            throw new InvalidOperationException($"Scene system {id} does not exist.");
        
        system.OnUnregister();
    }


    // Explicit call to remove any dependencies to SystemUpdateStage
    internal void PreUpdate()
    {
        foreach (Entity e in _entities)
            if (e.EnabledInHierarchy)
                e.EnsureComponentInitialization();
    }
    
    
    internal void Update(EntityUpdateStage stage)
    {
        if (_isBeingDestroyed)
            return;

        foreach (Entity entity in _entities)
        {
            if (entity.IsDestroyed)
                throw new InvalidOperationException($"Entity {entity.InstanceID} has been destroyed.");

            if (!entity.Enabled)
                return;
            
            // Child entities are updated recursively by their parents.
            if (!entity.IsRootEntity)
                continue;
            
            entity.UpdateComponentsRecursive(stage);
            entity.UpdateSystemsRecursive(stage);
        }
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.Update(stage);
    }
    
    
    internal void Destroy()
    {
        _isBeingDestroyed = true;
        
        // Destroy all scene systems.
        foreach (SceneSystem system in _sceneSystems.Values)
            system.OnUnregister();
        
        // Destroy all entities (includes their systems and components).
        foreach (Entity entity in _entities)
            entity.Destroy();
    }
}
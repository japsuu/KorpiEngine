using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Manages all entities, components, and systems in a scene.
/// </summary>
internal sealed class EntityScene
{
    private readonly List<Entity> _entities = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<SceneSystemID, SceneSystem> _sceneSystems = [];


    internal void RegisterEntity(Entity entity)
    {
        if (entity.ComponentCount > 0)
            throw new InvalidOperationException($"Entity {entity} has components before being registered. This is not allowed.");
        
        if (entity.SystemCount > 0)
            throw new InvalidOperationException($"Entity {entity} has systems before being registered. This is not allowed.");
        
        _entities.Add(entity);
    }


    internal void UnregisterEntity(Entity entity)
    {
        _entities.Remove(entity);
    }


    internal void RegisterComponent(EntityComponent component)
    {
        _components.Add(component);
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryRegisterComponent(component);
    }


    internal void UnregisterComponent(EntityComponent component)
    {
        if (!_components.Remove(component))
            throw new InvalidOperationException($"Component {component} is not registered.");
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryUnregisterComponent(component);
    }


    internal void RegisterSceneSystem<T>() where T : SceneSystem, new()
    {
        SceneSystem system = new T();
        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.TryAdd(id, system))
            throw new InvalidOperationException($"Scene system {id} already exists. Scene systems are singletons by default.");
        
        system.OnRegister(_components);
    }
    
    
    internal void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.Remove(id, out SceneSystem? system))
            throw new InvalidOperationException($"Scene system {id} does not exist.");
        
        system.OnUnregister();
    }
    
    
    internal void Update(SystemUpdateStage stage)
    {
        foreach (Entity entity in _entities)
        {
            // Child entities are updated recursively by their parents.
            if (!entity.IsSpatialRootEntity)
                continue;
            
            entity.UpdateRecursive(stage);
        }
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.Update(stage);
    }
    
    
    internal void Destroy()
    {
        // Destroy all scene systems.
        foreach (SceneSystem system in _sceneSystems.Values)
            system.OnUnregister();
        
        // Destroy all entities (includes their systems and components).
        foreach (Entity entity in _entities)
            entity.Destroy();
    }
}
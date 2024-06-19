using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

public static class EntityWorld
{
    private static readonly List<Entity> Entities = [];
    private static readonly List<EntityComponent> Components = [];
    private static readonly Dictionary<WorldSystemID, WorldSystem> WorldSystems = [];


    internal static void RegisterEntity(Entity entity) => Entities.Add(entity);
    internal static void UnregisterEntity(Entity entity) => Entities.Remove(entity);
    
    
    internal static void RegisterComponent(EntityComponent component)
    {
        Components.Add(component);
        
        foreach (WorldSystem system in WorldSystems.Values)
            system.TryRegisterComponent(component);
    }


    internal static void UnregisterComponent(EntityComponent component)
    {
        if (!Components.Remove(component))
            throw new InvalidOperationException($"Component {component} is not registered.");
        
        foreach (WorldSystem system in WorldSystems.Values)
            system.TryUnregisterComponent(component);
    }


    internal static void RegisterWorldSystem<T>() where T : WorldSystem, new()
    {
        WorldSystem system = new T();
        WorldSystemID id = WorldSystemID.Generate<T>();
        
        if (!WorldSystems.TryAdd(id, system))
            throw new InvalidOperationException($"World system {id} already exists. World systems are singletons by default.");
        
        system.OnRegister(Components);
    }
    
    
    internal static void UnregisterWorldSystem<T>() where T : WorldSystem
    {
        WorldSystemID id = WorldSystemID.Generate<T>();
        
        if (!WorldSystems.Remove(id, out WorldSystem? system))
            throw new InvalidOperationException($"World system {id} does not exist.");
        
        system.OnUnregister();
    }
    
    
    internal static void Update(SystemUpdateStage stage)
    {
        foreach (Entity entity in Entities)
            entity.Update(stage);
        
        foreach (WorldSystem system in WorldSystems.Values)
            system.Update(stage);
    }
    
    
    internal static void Destroy()
    {
        // Destroy all world systems.
        foreach (WorldSystem system in WorldSystems.Values)
            system.OnUnregister();
        
        // Destroy all entities (includes their systems and components).
        foreach (Entity entity in Entities)
            entity.Destroy();
    }
}
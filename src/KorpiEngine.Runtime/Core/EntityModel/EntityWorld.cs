using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

public static class EntityWorld
{
    public static event Action<EntityComponent>? ComponentRegistered;
    
    private static readonly List<Entity> Entities = [];
    private static readonly Dictionary<WorldSystemID, WorldSystem> WorldSystems = [];


    internal static void RegisterEntity(Entity entity) => Entities.Add(entity);
    internal static void UnregisterEntity(Entity entity) => Entities.Remove(entity);
    
    
    internal static void RegisterComponent(EntityComponent component) => ComponentRegistered?.Invoke(component);
    internal static void UnregisterComponent(EntityComponent component) { }


    internal static void RegisterWorldSystem<T>() where T : WorldSystem, new()
    {
        WorldSystem system = new T();
        WorldSystemID id = WorldSystemID.Generate<T>();
        
        WorldSystems.Add(id, system);
    }
    
    
    internal static void UnregisterWorldSystem<T>() where T : WorldSystem
    {
        WorldSystemID id = WorldSystemID.Generate<T>();
        
        WorldSystems.Remove(id);
    }
    
    
    internal static void Update(SystemUpdateStage stage)
    {
        foreach (Entity entity in Entities)
            entity.Update(stage);
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
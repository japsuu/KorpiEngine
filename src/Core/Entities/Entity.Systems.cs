using KorpiEngine.Utils;

namespace KorpiEngine.Entities;

public sealed partial class Entity
{
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

        _scene?.RegisterComponent(component);
    }


    private void UnregisterComponentWithSystems(EntityComponent component)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.TryUnregisterComponent(component);

        _scene?.UnregisterComponent(component);
    }
}
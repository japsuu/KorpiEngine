using Arch.System;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Represents a group of <see cref="NativeSystem"/>s that are all updated together.
/// </summary>
public class SceneSystemGroup
{
    private readonly Group<double> _systems;


    public SceneSystemGroup(string name)
    {
        _systems = new Group<double>(name);
    }
    
    
    public SceneSystemGroup Add(params NativeSystem[] systems)
    {
        foreach (NativeSystem system in systems)
            Add(system);

        return this;
    }
    
    
    public SceneSystemGroup Add<T>() where T : NativeSystem, new()
    {
        return Add(new T());
    }

    
    public SceneSystemGroup Add(NativeSystem system)
    {
        _systems.Add(system);

        return this;
    }


    public void Initialize()
    {
        _systems.Initialize();
    }


    public void BeforeUpdate(double deltaTime)
    {
        _systems.BeforeUpdate(deltaTime);
    }


    public void Update(double deltaTime)
    {
        _systems.Update(deltaTime);
    }


    public void AfterUpdate(double deltaTime)
    {
        _systems.AfterUpdate(deltaTime);
    }


    public void Dispose()
    {
        _systems.Dispose();
    }
}
using Arch.Core;
using Arch.System;
using KorpiEngine.Core.ECS.Components;
using KorpiEngine.Core.ECS.Systems;
using KorpiEngine.Core.GameObjects;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="GameObject"/>s and register systems.
/// </summary>
public abstract class Scene : IDisposable
{
    /// <summary>
    /// The systems for this scene.
    /// </summary>
    private readonly Group<double> _systems;
    /// <summary>
    /// The game object manager for this scene.
    /// </summary>
    internal readonly GameObjectManager GameObjectManager;
    
    /// <summary>
    /// The ESC world for this scene.
    /// </summary>
    internal readonly World World;


    protected Scene()
    {
        World = World.Create();
        _systems = new Group<double>("SceneSystems");
        GameObjectManager = new GameObjectManager();
    }
    
    
    protected GameObject CreateGameObject(bool isEnabled)
    {
        return GameObjectManager.CreateGameObject(World.Create<Position, Rotation>(), isEnabled);
    }
    
    
    internal void InternalLoad()
    {
        RegisterSystems(_systems);

        // Add the render system last, so it draws last.
        _systems.Add(new RenderSystem(this));
        _systems.Initialize();
        
        Load();
    }
    
    
    internal void InternalUnload()
    {
        _systems.Dispose();
        
        Unload();
        
        World.Destroy(World);
    }
    
    
    internal void InternalEarlyUpdate()
    {
        _systems.BeforeUpdate(Time.DeltaTime);
        
        EarlyUpdate();
    }
    
    
    internal void InternalUpdate()
    {
        _systems.Update(Time.DeltaTime);
        
        Update();
        GameObjectManager.Update();
    }
    
    
    internal void InternalFixedUpdate()
    {
        FixedUpdate();
        GameObjectManager.FixedUpdate();
    }
    
    
    internal void InternalDraw()
    {
        LateUpdate();
        
        _systems.AfterUpdate(Time.DeltaTime);
    }
    
    
    /// <summary>
    /// Register new systems to the scene.
    /// Called before <see cref="Load"/>
    /// The systems will be automatically updated, in the order they were registered.
    /// </summary>
    protected virtual void RegisterSystems(Group<double> systems) { }
    
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void Load() { }
    
    /// <summary>
    /// Called every frame before <see cref="Update"/>
    /// </summary>
    protected virtual void EarlyUpdate() { }
    
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void Update() { }
    
    /// <summary>
    /// Called every frame after <see cref="Update"/>, just before the scene is drawn.
    /// </summary>
    protected virtual void LateUpdate() { }
    
    /// <summary>
    /// Called from the fixed update loop.
    /// </summary>
    protected virtual void FixedUpdate() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void Unload() { }
    
    
    public void Dispose()
    {
        _systems.Dispose();
        World.Dispose();
        GC.SuppressFinalize(this);
    }
}
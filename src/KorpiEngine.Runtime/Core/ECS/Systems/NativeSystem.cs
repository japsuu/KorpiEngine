using Arch.Core;
using Arch.System;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Represents a system that processes <see cref="Entity"/>s and their <see cref="INativeComponent"/>s in a scene.
/// </summary>
public abstract class NativeSystem(Scene scene) : IDisposable
{
    protected readonly Scene Scene = scene;
    protected readonly World World = scene.World;


    public void Initialize()
    {
        OnInitialize();
    }


    public void Update()
    {
        OnEarlyUpdate();
        OnUpdate();
        OnLateUpdate();
    }


    public void FixedUpdate()
    {
        OnEarlyFixedUpdate();
        OnFixedUpdate();
        OnLateFixedUpdate();
    }


    public void Draw()
    {
        OnEarlyDraw();
        OnDraw();
        OnLateDraw();
    }


    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }


    protected virtual void OnInitialize() { }
    
    protected virtual void OnEarlyUpdate() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnLateUpdate() { }
    
    protected virtual void OnEarlyFixedUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnLateFixedUpdate() { }
    
    protected virtual void OnEarlyDraw() { }
    protected virtual void OnDraw() { }
    protected virtual void OnLateDraw() { }
    
    protected virtual void OnDispose() { }
}
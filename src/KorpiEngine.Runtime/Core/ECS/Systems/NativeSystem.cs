using Arch.System;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

internal abstract class NativeSystem(Scene scene) : BaseNativeSystem(scene)
{
    public void Initialize()
    {
        OnInitialize();
    }


    public sealed override void Update()
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
}
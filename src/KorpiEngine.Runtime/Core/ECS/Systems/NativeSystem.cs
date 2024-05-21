using Arch.Core;
using Arch.System;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Represents a system that processes <see cref="Entity"/>s and their <see cref="INativeComponent"/>s in a scene.
/// </summary>
public abstract class NativeSystem : BaseSystem<World, double>
{
    protected Scene Scene;
    protected NativeSystem(Scene scene) : base(scene.World)
    {
        Scene = scene;
    }


    public sealed override void Initialize()
    {
        base.Initialize();
        SystemInitialize();
    }


    public sealed override void BeforeUpdate(in double t)
    {
        base.BeforeUpdate(in t);
        SystemEarlyUpdate(in t);
    }


    public sealed override void Update(in double t)
    {
        base.Update(in t);
        SystemUpdate(in t);
    }


    public sealed override void AfterUpdate(in double t)
    {
        base.AfterUpdate(in t);
        SystemLateUpdate(in t);
    }


    protected virtual void SystemInitialize() { }
    protected virtual void SystemEarlyUpdate(in double deltaTime) { }
    protected virtual void SystemUpdate(in double deltaTime) { }
    protected virtual void SystemLateUpdate(in double deltaTime) { }
}
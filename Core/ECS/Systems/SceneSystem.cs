using Arch.Core;
using Arch.System;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// An ECS system that is part of a scene.
/// </summary>
public abstract class SceneSystem : BaseSystem<World, double>
{
    private readonly Scene _scene;


    protected SceneSystem(Scene scene) : base(scene.World)
    {
        _scene = scene;
    }
}
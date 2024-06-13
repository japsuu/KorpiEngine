using Arch.Core;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

internal abstract class BaseNativeSystem(Scene scene) : IDisposable
{
    protected readonly Scene Scene = scene;
    protected readonly World World = scene.World;


    public virtual void Update() { }


    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }

    
    protected virtual void OnDispose() { }
}
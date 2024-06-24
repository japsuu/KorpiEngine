using KorpiEngine.Core.EntityModel.Components;

namespace KorpiEngine.Core.EntityModel.Systems.World;

public class MeshRenderSceneSystem : SceneSystem
{
    private readonly List<MeshRendererComponent> _components = [];
    
    
    public override void TryRegisterComponent<T>(T c)
    {
        if (c is MeshRendererComponent meshRenderer)
            _components.Add(meshRenderer);
    }


    public override void TryUnregisterComponent<T>(T c)
    {
        if (c is MeshRendererComponent meshRenderer)
            _components.Remove(meshRenderer);
    }


    public override void Update(SystemUpdateStage stage)
    {
        if (stage != SystemUpdateStage.Render)
            return;
        
        foreach (MeshRendererComponent c in _components)
            c.Render();
    }
}
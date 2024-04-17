using Arch.Core;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Renders all meshes in the scene.
/// </summary>
internal class MeshRenderSystem : NativeSystem
{
    private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<TransformComponent, MeshRendererComponent>();
    
    public MeshRenderSystem(Scene scene) : base(scene) { }


    protected override void SystemUpdate(in double deltaTime)
    {
        World.Query(in _queryDescription, (ref TransformComponent transform, ref MeshRendererComponent mesh) =>
        {
            if (mesh.Mesh == null)
                return;
            
            Material mat = mesh.Material ?? MaterialManager.MissingMaterial3D;
            
            Renderer3D.RenderMesh(mesh.Mesh, mat, transform.Matrix);
        });
    }
}
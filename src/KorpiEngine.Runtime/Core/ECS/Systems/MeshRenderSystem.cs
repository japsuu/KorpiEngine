using Arch.Core;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Rendering;
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
        #warning Switch to inline queries for zero-allocation
        World.Query(in _queryDescription, (ref TransformComponent transform, ref MeshRendererComponent mesh) =>
        {
            if (mesh.Mesh == null)
                return;
            
            Material mat = mesh.Material ?? new Material(Shader.Find("Defaults/Standard.shader"));

            for (int i = 0; i < mat.PassCount; i++)
            {
                mat.SetPass(i);
                Graphics.DrawMeshNow(mesh.Mesh, transform.Matrix, mat);
            }
        });
    }
}
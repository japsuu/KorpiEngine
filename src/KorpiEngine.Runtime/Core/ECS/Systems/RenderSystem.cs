using Arch.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Renders everything in the scene.
/// </summary>
internal class RenderSystem(Scene scene) : NativeSystem(scene)
{
    private readonly QueryDescription _meshRendererQueryDescription = new QueryDescription().WithAll<TransformComponent, MeshRendererComponent>();
    private MeshRenderQuery _meshRendererQuery;


    protected override void SystemEarlyUpdate(in double deltaTime)
    {
    }


    protected override void SystemUpdate(in double deltaTime)
    {
        World.InlineQuery<MeshRenderQuery, TransformComponent, MeshRendererComponent>(in _meshRendererQueryDescription, ref _meshRendererQuery);
    }


    protected override void SystemLateUpdate(in double deltaTime)
    {
    }


    private struct MeshRenderQuery : IForEach<TransformComponent, MeshRendererComponent>
    {
        public void Update(ref TransformComponent transform, ref MeshRendererComponent mesh)
        {
            if (mesh.Mesh == null)
                return;

            Material mat = mesh.Material ?? new Material(Shader.Find("Defaults/Invalid.shader"));

            for (int i = 0; i < mat.PassCount; i++)
            {
                mat.SetPass(i);
                Matrix4x4 matrix = Matrix4x4.TRS(transform.Position, transform.Rotation, transform.Scale);
                Graphics.DrawMeshNow(mesh.Mesh, transform.Matrix, mat);
            }
        }
    }
}
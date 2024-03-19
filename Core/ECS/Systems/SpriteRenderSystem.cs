using Arch.Core;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// The default system for rendering entities with a <see cref="SpriteRendererComponent"/>.
/// </summary>
public class SpriteRenderSystem : NativeSystem
{
    private readonly QueryDescription _desc = new QueryDescription().WithAll<TransformComponent, SpriteRendererComponent>();
    
    
    public SpriteRenderSystem(Scene scene) : base(scene) { }

    protected override void SystemUpdate(in double deltaTime)
    {
        World.Query(in _desc, (ref TransformComponent transform, ref SpriteRendererComponent meshData) => {
            // Render the mesh
            Renderer3D.DrawQuad(transform, meshData.Color);
        });
    }
}
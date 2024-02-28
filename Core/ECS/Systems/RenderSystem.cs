using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// System for rendering meshes.
/// </summary>
internal class RenderSystem : SceneSystem
{
    public RenderSystem(Scene scene) : base(scene)
    {
    }
}
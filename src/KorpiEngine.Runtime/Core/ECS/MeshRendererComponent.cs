using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.ECS;

public struct MeshRendererComponent : INativeComponent
{
    public Mesh? Mesh;
    public Material? Material;
}
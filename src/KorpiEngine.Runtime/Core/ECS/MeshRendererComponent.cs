using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;

namespace KorpiEngine.Core.ECS;

public struct MeshRendererComponent : INativeComponent
{
    public Mesh? Mesh;
    public Material? Material;
}
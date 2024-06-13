using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.Internal.AssetManagement;

namespace KorpiEngine.Core.ECS;

public struct MeshRendererComponent : INativeComponent
{
    public AssetRef<Mesh> Mesh;
    public AssetRef<Material> Material;
}
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;

namespace KorpiEngine.Core.EntityModel.Components;

public class MeshRendererComponent : SpatialEntityComponent
{
    public Mesh? Mesh;
    public Material? Material;
}
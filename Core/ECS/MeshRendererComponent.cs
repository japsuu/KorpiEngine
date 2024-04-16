using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;

namespace KorpiEngine.Core.ECS;

public struct MeshRendererComponent : INativeComponent
{
    public Mesh? Mesh;
    public Material? Material;


    public MeshRendererComponent()
    {
        Mesh = null;
        Material = new StandardMaterial3D();
    }
}
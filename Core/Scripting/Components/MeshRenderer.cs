using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;

namespace KorpiEngine.Core.Scripting.Components;

/// <summary>
/// Contains the mesh and material data for an <see cref="Entity"/>.
/// </summary>
public class MeshRenderer : Component
{
    public Mesh Mesh { get; private set; }
    public MaterialInstance Material { get; private set; }
    
    
    public void SetMesh(Mesh mesh)
    {
        Mesh = mesh;
    }
}
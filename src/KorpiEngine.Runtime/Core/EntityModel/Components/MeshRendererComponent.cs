using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.EntityModel.Components;

public class MeshRendererComponent : SpatialEntityComponent
{
    public Mesh? Mesh;
    public Material? Material;


    public void Render()
    {
        if (Mesh == null)
            return;
            
        Material mat = Material ?? new Material(Shader.Find("Defaults/Invalid.shader"));
        
        for (int i = 0; i < mat.PassCount; i++)
        {
            mat.SetPass(i);
            Graphics.DrawMeshNow(Mesh, Transform.LocalToWorldMatrix, mat);  //WARN: Test matrix!
        }
    }
}
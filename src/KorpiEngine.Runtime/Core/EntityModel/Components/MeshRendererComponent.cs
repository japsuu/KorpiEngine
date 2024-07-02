using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.EntityModel.Components;

public class MeshRendererComponent : EntityComponent
{
    public ResourceRef<Mesh> Mesh;
    public ResourceRef<Material> Material;

    
    protected override void OnRenderObject()
    {
        if (!Mesh.IsAvailable)
            return;
            
        Material mat = Material.Res ?? new Material(Shader.Find("Defaults/Invalid.shader"));
        
        for (int i = 0; i < mat.PassCount; i++)
        {
            mat.SetPass(i);
            Graphics.DrawMeshNow(Mesh.Res!, Transform, mat);
        }
    }

    
    protected override void OnRenderObjectDepth()
    {
        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            Matrix4x4 mat = Graphics.GetCamRelativeTransform(Transform);

            Matrix4x4 mvp = Matrix4x4.Identity;
            mvp = Matrix4x4.Multiply(mvp, mat);
            mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
            mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
            Material.Res!.SetMatrix("_MatMVP", mvp);
            Material.Res!.SetShadowPass(true);
            Graphics.DrawMeshNowDirect(Mesh.Res!);
        }
    }
}
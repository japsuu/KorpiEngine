using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel.Components;

public class MeshRendererComponent : EntityComponent
{
    public ResourceRef<Mesh> Mesh;
    public ResourceRef<Material> Material;
    public Color MainColor = Color.White;
    
    private readonly Dictionary<int, Matrix4x4> _previousTransforms = new();
    private static Material? invalidMaterial;

    
    protected override void OnRenderObject()
    {
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        int camID = CameraComponent.RenderingCamera.InstanceID;
        
        _previousTransforms.TryAdd(camID, transform);
        Matrix4x4 previousTransform = _previousTransforms[camID];
        
        if (!Mesh.IsAvailable)
            return;
            
        Material? material = Material.Res;
        if (material == null)
        {
            invalidMaterial ??= new Material(Shader.Find("Defaults/Invalid.shader"), "invalid material");
            material = invalidMaterial;
        }
        
        if (Mesh.IsAvailable)
        {
            material.SetColor("_MainColor", MainColor);
            material.SetInt("_ObjectID", Entity.InstanceID);
            for (int i = 0; i < material.PassCount; i++)
            {
                material.SetPass(i);
                Graphics.DrawMeshNow(Mesh.Res!, transform, material, previousTransform);
            }
        }

        _previousTransforms[camID] = transform;
    }

    
    protected override void OnRenderObjectDepth()
    {
        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;

            Matrix4x4 mvp = Matrix4x4.Identity;
            mvp = Matrix4x4.Multiply(mvp, transform);
            mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
            mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
            Material.Res!.SetMatrix("_MatMVP", mvp);
            Material.Res!.SetShadowPass(true);
            Graphics.DrawMeshNowDirect(Mesh.Res!);
        }
    }
}
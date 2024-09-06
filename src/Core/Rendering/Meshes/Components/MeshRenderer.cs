using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;

namespace KorpiEngine.Rendering;

public class MeshRenderer : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public AssetRef<Mesh> Mesh { get; set; }
    public AssetRef<Material> Material { get; set; }
    public ColorHDR MainColor { get; set; } = ColorHDR.White;
    
    private readonly Dictionary<int, Matrix4x4> _previousTransforms = new();

    
    protected override void OnRenderObject()
    {
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        int camID = Camera.RenderingCamera.InstanceID;
        
        _previousTransforms.TryAdd(camID, transform);
        Matrix4x4 previousTransform = _previousTransforms[camID];
        
        if (!Mesh.IsAvailable)
            return;
            
        Material? material = Material.Res;
        if (material == null)
        {
            material = Rendering.Material.InvalidMaterial.Res!;
#if TOOLS
            Application.Logger.Warn($"Material for {Entity.Name} is null, using invalid material");
#endif
        }
        
        if (Graphics.FrustumTest(Mesh.Res!.BoundingSphere, transform))
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

    
    protected override void OnRenderDepth()
    {
        if (!Mesh.IsAvailable || !Material.IsAvailable)
            return;
        
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        
        if (!Graphics.FrustumTest(Mesh.Res!.BoundingSphere, transform))
            return;

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, transform);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        Material.Res!.SetMatrix("_MatMVP", mvp);
        Material.Res!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(Mesh.Res!);
    }
}
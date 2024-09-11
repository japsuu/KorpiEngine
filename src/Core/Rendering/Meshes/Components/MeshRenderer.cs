using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;

namespace KorpiEngine.Rendering;

public class MeshRenderer : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public ColorHDR MainColor { get; set; } = ColorHDR.White;
    public Mesh? Mesh
    {
        get => _mesh?.Asset;
        set
        {
            if (_mesh != null)
                _mesh.Release();
            
            _mesh = new AssetReference<Mesh>(value);
        }
    }

    public Material? Material
    {
        get => _material?.Asset;
        set
        {
            if (_material != null)
                _material.Release();
            
            _material = new AssetReference<Material>(value);
        }
    }

    private readonly Dictionary<int, Matrix4x4> _previousTransforms = new();
    private AssetReference<Mesh>? _mesh;
    private AssetReference<Material>? _material;

    
    protected override void OnRenderObject()
    {
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        int camID = Camera.RenderingCamera.InstanceID;
        
        _previousTransforms.TryAdd(camID, transform);
        Matrix4x4 previousTransform = _previousTransforms[camID];
        
        if (Mesh == null)
            return;
        
        Material? material = Material;
        if (material == null)
        {
            material = Material.InvalidMaterial;
#if TOOLS
            Application.Logger.Warn($"Material for {Entity.Name} is null, using invalid material");
#endif
        }
        
        if (Graphics.FrustumTest(Mesh.BoundingSphere, transform))
        {
            material.SetColor("_MainColor", MainColor);
            material.SetInt("_ObjectID", Entity.InstanceID);
            for (int i = 0; i < material.PassCount; i++)
            {
                material.SetPass(i);
                Graphics.DrawMeshNow(Mesh, transform, material, previousTransform);
            }
        }

        _previousTransforms[camID] = transform;
    }

    
    protected override void OnRenderDepth()
    {
        if (Mesh == null || Material == null)
            return;
        
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        
        if (!Graphics.FrustumTest(Mesh.BoundingSphere, transform))
            return;

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, transform);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        Material.SetMatrix("_MatMVP", mvp);
        Material.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(Mesh);
    }


    protected override void OnDestroy()
    {
        _mesh?.Release();
        _material?.Release();
    }
}
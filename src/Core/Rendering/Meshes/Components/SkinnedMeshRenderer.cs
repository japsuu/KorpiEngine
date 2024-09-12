using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Tools.Serialization;

namespace KorpiEngine.Rendering;

public class SkinnedMeshRenderer : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public Mesh? Mesh
    {
        get => _mesh?.Asset;
        set
        {
            if (_mesh?.Asset == value)
                return;
            
            _mesh?.Release();
            _mesh = value?.CreateReference();
        }
    }

    public Material? Material
    {
        get => _material?.Asset;
        set
        {
            if (_material?.Asset == value)
                return;
            
            _material?.Release();
            _material = value?.CreateReference();
        }
    }

    public Transform?[] Bones { get; set; } = [];

    private Matrix4x4[]? _boneTransforms;
    private AssetReference<Mesh>? _mesh;
    private AssetReference<Material>? _material;


    private void GetBoneMatrices()
    {
        _boneTransforms = new Matrix4x4[Bones.Length];
        for (int i = 0; i < Bones.Length; i++)
        {
            Transform? t = Bones[i];
            if (t == null)
                _boneTransforms[i] = Matrix4x4.Identity;
            else
                _boneTransforms[i] = t.LocalToWorldMatrix * Entity.Transform.WorldToLocalMatrix;
        }
    }


    private readonly Dictionary<int, Matrix4x4> _prevMats = new();


    protected override void OnRenderObject()
    {
        // Store the current camera-relative transform to be used in the next frame
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        int camID = Camera.RenderingCamera.InstanceID;
        if (!_prevMats.ContainsKey(camID))
            _prevMats[camID] = Entity.GlobalCameraRelativeTransform;
        Matrix4x4 prevMat = _prevMats[camID];

        if (Mesh != null && Material != null)
        {
            if (!Graphics.FrustumTest(Mesh.BoundingSphere, transform))
                return;

            GetBoneMatrices();
            Material.EnableKeyword("SKINNED");
            Material.SetInt("_ObjectID", Entity.InstanceID);
            Material.SetMatrices("_BindPoses", Mesh.BindPoses!);
            Material.SetMatrices("_BoneTransforms", _boneTransforms!);
            for (int i = 0; i < Material.PassCount; i++)
            {
                Material.SetPass(i);
                Graphics.DrawMeshNow(Mesh, transform, Material, prevMat);
            }

            Material.DisableKeyword("SKINNED");
        }

        _prevMats[camID] = transform;
    }


    protected override void OnRenderDepth()
    {
        if (Mesh == null || Material == null)
            return;
        
        GetBoneMatrices();
        Material.EnableKeyword("SKINNED");
        Material.SetMatrices("_BindPoses", Mesh.BindPoses!);
        Material.SetMatrices("_BoneTransforms", _boneTransforms!);

        Matrix4x4 mvp = Matrix4x4.Identity;
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        
        if (!Graphics.FrustumTest(Mesh.BoundingSphere, transform))
            return;
        
        mvp = Matrix4x4.Multiply(mvp, transform);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        Material.SetMatrix("_MatMVP", mvp);
        Material.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(Mesh);

        Material.DisableKeyword("SKINNED");
    }


    protected override void OnDestroy()
    {
        _mesh?.Release();
        _material?.Release();
    }
}
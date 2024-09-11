using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Tools.Serialization;

namespace KorpiEngine.Rendering;

public class SkinnedMeshRenderer : EntityComponent, ISerializable
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public ExternalAssetRef<Mesh> Mesh { get; set; }
    public ExternalAssetRef<Material> Material { get; set; }

    public Transform?[] Bones { get; set; } = [];

    private Matrix4x4[]? _boneTransforms;


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

        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            if (!Graphics.FrustumTest(Mesh.Asset!.BoundingSphere, transform))
                return;

            GetBoneMatrices();
            Material.Asset!.EnableKeyword("SKINNED");
            Material.Asset!.SetInt("_ObjectID", Entity.InstanceID);
            Material.Asset!.SetMatrices("_BindPoses", Mesh.Asset!.BindPoses!);
            Material.Asset!.SetMatrices("_BoneTransforms", _boneTransforms!);
            for (int i = 0; i < Material.Asset!.PassCount; i++)
            {
                Material.Asset!.SetPass(i);
                Graphics.DrawMeshNow(Mesh.Asset!, transform, Material.Asset!, prevMat);
            }

            Material.Asset!.DisableKeyword("SKINNED");
        }

        _prevMats[camID] = transform;
    }


    protected override void OnRenderDepth()
    {
        if (!Mesh.IsAvailable || !Material.IsAvailable)
            return;
        
        GetBoneMatrices();
        Material.Asset!.EnableKeyword("SKINNED");
        Material.Asset!.SetMatrices("_BindPoses", Mesh.Asset!.BindPoses!);
        Material.Asset!.SetMatrices("_BoneTransforms", _boneTransforms!);

        Matrix4x4 mvp = Matrix4x4.Identity;
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        
        if (!Graphics.FrustumTest(Mesh.Asset!.BoundingSphere, transform))
            return;
        
        mvp = Matrix4x4.Multiply(mvp, transform);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        Material.Asset!.SetMatrix("_MatMVP", mvp);
        Material.Asset!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(Mesh.Asset!);

        Material.Asset!.DisableKeyword("SKINNED");
    }


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();
        compoundTag.Add("Mesh", Serializer.Serialize(Mesh, ctx));
        compoundTag.Add("Material", Serializer.Serialize(Material, ctx));
        compoundTag.Add("Bones", Serializer.Serialize(Bones, ctx));

        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        Mesh = Serializer.Deserialize<ExternalAssetRef<Mesh>>(value["Mesh"], ctx);
        Material = Serializer.Deserialize<ExternalAssetRef<Material>>(value["Material"], ctx);
        Bones = Serializer.Deserialize<Transform[]>(value["Bones"], ctx)!;
    }
}
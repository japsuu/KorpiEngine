using KorpiEngine.AssetManagement;
using KorpiEngine.EntityModel.SpatialHierarchy;
using KorpiEngine.Rendering;
using KorpiEngine.Rendering.Cameras;
using KorpiEngine.Rendering.Materials;
using KorpiEngine.Serialization;

namespace KorpiEngine.EntityModel.Components;

public class SkinnedMeshRenderer : EntityComponent, ISerializable
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public ResourceRef<Mesh> Mesh { get; set; }
    public ResourceRef<Material> Material { get; set; }

    public Transform?[] Bones { get; set; } = [];

    private System.Numerics.Matrix4x4[]? _boneTransforms;


    private void GetBoneMatrices()
    {
        _boneTransforms = new System.Numerics.Matrix4x4[Bones.Length];
        for (int i = 0; i < Bones.Length; i++)
        {
            Transform? t = Bones[i];
            if (t == null)
                _boneTransforms[i] = System.Numerics.Matrix4x4.Identity;
            else
                _boneTransforms[i] = (t.LocalToWorldMatrix * Entity.Transform.WorldToLocalMatrix).ToFloat();
        }
    }


    private readonly Dictionary<int, Matrix4x4> _prevMats = new();


    protected override void OnRenderObject()
    {
        // Store the current camera-relative transform to be used in the next frame
        Matrix4x4 mat = Entity.GlobalCameraRelativeTransform;
        int camID = Camera.RenderingCamera.InstanceID;
        if (!_prevMats.ContainsKey(camID))
            _prevMats[camID] = Entity.GlobalCameraRelativeTransform;
        Matrix4x4 prevMat = _prevMats[camID];

        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            GetBoneMatrices();
            Material.Res!.EnableKeyword("SKINNED");
            Material.Res!.SetInt("_ObjectID", Entity.InstanceID);
            Material.Res!.SetMatrices("_BindPoses", Mesh.Res!.BindPoses!);
            Material.Res!.SetMatrices("_BoneTransforms", _boneTransforms!);
            for (int i = 0; i < Material.Res!.PassCount; i++)
            {
                Material.Res!.SetPass(i);
                Graphics.DrawMeshNow(Mesh.Res!, mat, Material.Res!, prevMat);
            }

            Material.Res!.DisableKeyword("SKINNED");
        }

        _prevMats[camID] = mat;
    }


    protected override void OnRenderDepth()
    {
        if (!Mesh.IsAvailable || !Material.IsAvailable)
            return;
        
        GetBoneMatrices();
        Material.Res!.EnableKeyword("SKINNED");
        Material.Res!.SetMatrices("_BindPoses", Mesh.Res!.BindPoses!);
        Material.Res!.SetMatrices("_BoneTransforms", _boneTransforms!);

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, Entity.GlobalCameraRelativeTransform);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        Material.Res!.SetMatrix("_MatMVP", mvp);
        Material.Res!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(Mesh.Res!);

        Material.Res!.DisableKeyword("SKINNED");
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
        Mesh = Serializer.Deserialize<ResourceRef<Mesh>>(value["Mesh"], ctx);
        Material = Serializer.Deserialize<ResourceRef<Material>>(value["Material"], ctx);
        Bones = Serializer.Deserialize<Transform[]>(value["Bones"], ctx)!;
    }
}
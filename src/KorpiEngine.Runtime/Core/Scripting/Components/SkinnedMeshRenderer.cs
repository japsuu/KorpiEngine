using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using Prowl.Runtime;

namespace KorpiEngine.Core.Scripting.Components;

public class SkinnedMeshRenderer : Behaviour, ISerializable
{
    public override RenderingOrder RenderOrder => RenderingOrder.Opaque;

    public AssetRef<Mesh> Mesh;
    public AssetRef<Material> Material;

    public Transform[] Bones = [];

    private System.Numerics.Matrix4x4[] boneTransforms;

    void GetBoneMatrices()
    {
        boneTransforms = new System.Numerics.Matrix4x4[Bones.Length];
        for (int i = 0; i < Bones.Length; i++)
        {
            var t = Bones[i];
            if (t == null)
            {
                boneTransforms[i] = System.Numerics.Matrix4x4.Identity;
            }
            else
            {
                boneTransforms[i] = (t.LocalToWorldMatrix * Entity.Transform.WorldToLocalMatrix).ToFloat();
            }
        }
    }

    private Dictionary<int, Matrix4x4> prevMats = new();

    public override void OnRenderObject()
    {
        var mat = Entity.GlobalCamRelative;
        int camID = Camera.RenderingCamera.InstanceID;
        if (!prevMats.ContainsKey(camID)) prevMats[camID] = Entity.GlobalCamRelative;
        var prevMat = prevMats[camID];
        
        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            GetBoneMatrices();
            Material.Res!.EnableKeyword("SKINNED");
            Material.Res!.SetInt("ObjectID", Entity.InstanceID);
            Material.Res!.SetMatrices("bindPoses", Mesh.Res.BindPoses);
            Material.Res!.SetMatrices("boneTransforms", boneTransforms);
            for (int i = 0; i < Material.Res!.PassCount; i++)
            {
                Material.Res!.SetPass(i);
                Graphics.DrawMeshNow(Mesh.Res!, mat, Material.Res!/*, prevMat*/);
            }
            Material.Res!.DisableKeyword("SKINNED");
        }

        prevMats[camID] = mat;
    }

    public override void OnRenderObjectDepth()
    {
        if (Mesh.IsAvailable && Material.IsAvailable)
        {
            GetBoneMatrices();
            Material.Res!.EnableKeyword("SKINNED");
            Material.Res!.SetMatrices("bindPoses", Mesh.Res.BindPoses);
            Material.Res!.SetMatrices("boneTransforms", boneTransforms);

            var mvp = Matrix4x4.Identity;
            mvp = Matrix4x4.Multiply(mvp, Entity.GlobalCamRelative);
            mvp = Matrix4x4.Multiply(mvp, Graphics.MatDepthView);
            mvp = Matrix4x4.Multiply(mvp, Graphics.MatDepthProjection);
            Material.Res!.SetMatrix("mvp", mvp);
            Material.Res!.SetShadowPass(true);
            Graphics.DrawMeshNowDirect(Mesh.Res!);

            Material.Res!.DisableKeyword("SKINNED");
        }
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
        Mesh = Serializer.Deserialize<AssetRef<Mesh>>(value["Mesh"], ctx);
        Material = Serializer.Deserialize<AssetRef<Material>>(value["Material"], ctx);
        Bones = Serializer.Deserialize<Transform[]>(value["Bones"], ctx);
    }
}
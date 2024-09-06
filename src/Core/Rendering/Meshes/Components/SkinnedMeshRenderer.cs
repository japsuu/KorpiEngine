﻿using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Tools.Serialization;

namespace KorpiEngine.Rendering;

public class SkinnedMeshRenderer : EntityComponent, ISerializable
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.GeometryPass;

    public AssetRef<Mesh> Mesh { get; set; }
    public AssetRef<Material> Material { get; set; }

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
            if (!Graphics.FrustumTest(Mesh.Res!.BoundingSphere, transform))
                return;

            GetBoneMatrices();
            Material.Res!.EnableKeyword("SKINNED");
            Material.Res!.SetInt("_ObjectID", Entity.InstanceID);
            Material.Res!.SetMatrices("_BindPoses", Mesh.Res!.BindPoses!);
            Material.Res!.SetMatrices("_BoneTransforms", _boneTransforms!);
            for (int i = 0; i < Material.Res!.PassCount; i++)
            {
                Material.Res!.SetPass(i);
                Graphics.DrawMeshNow(Mesh.Res!, transform, Material.Res!, prevMat);
            }

            Material.Res!.DisableKeyword("SKINNED");
        }

        _prevMats[camID] = transform;
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
        Matrix4x4 transform = Entity.GlobalCameraRelativeTransform;
        
        if (!Graphics.FrustumTest(Mesh.Res!.BoundingSphere, transform))
            return;
        
        mvp = Matrix4x4.Multiply(mvp, transform);
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
        Mesh = Serializer.Deserialize<AssetRef<Mesh>>(value["Mesh"], ctx);
        Material = Serializer.Deserialize<AssetRef<Material>>(value["Material"], ctx);
        Bones = Serializer.Deserialize<Transform[]>(value["Bones"], ctx)!;
    }
}
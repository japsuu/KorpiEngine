using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// Renders everything in the scene.
/// </summary>
internal class RenderSystem(Scene scene) : BaseNativeSystem(scene)
{
    private readonly QueryDescription _cameraQuery = new QueryDescription().WithAll<CameraComponent>();
    
    private readonly QueryDescription _meshRendererQuery = new QueryDescription().WithAll<TransformComponent, MeshRendererComponent>();
    private readonly QueryDescription _skinnedMeshRendererQuery = new QueryDescription().WithAll<TransformComponent, SkinnedMeshRendererComponent>();
    private readonly QueryDescription _directionalLightQuery = new QueryDescription().WithAll<TransformComponent, DirectionalLightComponent>();
    
    private readonly List<(Entity e, CameraComponent c)> _cameras = [];


    public override void Update()
    {
        // Find all the cameras and sort them based on RenderPriority, and call the rendering functions for each.
        World.Query(in _cameraQuery, (ref Entity e, ref CameraComponent c) => _cameras.Add((e, c)));
        _cameras.Sort((a, b) => a.c.RenderPriority.CompareTo(b.c.RenderPriority));
        
        foreach ((Entity entity, CameraComponent camera) in _cameras)
        {
            Camera.RenderingCamera = Scripting.Entity.Wrap(entity.Reference(), Scene).GetComponent<Camera>();
            Draw();
        }
    }
    
    
    private void Draw()
    {
        OnEarlyDraw();
        OnDraw();
        OnLateDraw();
    }


    private void OnEarlyDraw()
    {
        UpdateAllLightShadowmaps();
    }


    private void OnDraw()
    {
        RenderAllOpaqueMeshes();
    }


    private void OnLateDraw()
    {
        RenderAllDirectionalLights();
    }


    private void UpdateAllLightShadowmaps()
    {
        World.Query(in _directionalLightQuery, (ref TransformComponent t, ref DirectionalLightComponent l) => UpdateDirectionalLightShadowmap(t, l));
    }


    private void RenderAllOpaqueMeshes()
    {
        World.Query(in _meshRendererQuery, (ref TransformComponent t, ref MeshRendererComponent m) => RenderOpaqueMesh(t, m));
        World.Query(in _skinnedMeshRendererQuery, (ref TransformComponent t, ref SkinnedMeshRendererComponent m) => RenderOpaqueSkinnedMesh(t, m));
    }


    private void RenderAllOpaqueMeshesDepth()
    {
        World.Query(in _meshRendererQuery, (ref TransformComponent t, ref MeshRendererComponent m) => RenderOpaqueMeshDepth(t, m));
        World.Query(in _skinnedMeshRendererQuery, (ref TransformComponent t, ref SkinnedMeshRendererComponent m) => RenderOpaqueSkinnedMeshDepth(t, m));
    }


    private void RenderAllDirectionalLights()
    {
        World.Query(in _directionalLightQuery, (ref TransformComponent t, ref DirectionalLightComponent l) => RenderDirectionalLight(t, l));
    }


    #region Normal meshes

    private void RenderOpaqueMesh(TransformComponent transform, MeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable)
            return;

        Material material = mesh.Material.Res ?? new Material(Shader.Find("Defaults/Invalid.shader"));

        Matrix4x4 matrix = Transform.GetLocalToWorldMatrix(transform);
        matrix.Translation -= Camera.RenderingCamera!.Entity.Transform.Position;

        for (int i = 0; i < material.PassCount; i++)
        {
            material.SetPass(i);
            Graphics.DrawMeshNow(mesh.Mesh.Res!, matrix, material);
        }
    }


    private void RenderOpaqueMeshDepth(TransformComponent transform, MeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable || !mesh.Material.IsAvailable)
            return;

        Matrix4x4 mat = Transform.GetLocalToWorldMatrix(transform);
        mat.Translation -= Camera.RenderingCamera!.Entity.Transform.Position;

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, mat);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        mesh.Material.Res!.SetMatrix("_MatMVP", mvp);
        mesh.Material.Res!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(mesh.Mesh.Res!);
    }

    #endregion


    #region Skinned meshes

    private void RenderOpaqueSkinnedMesh(TransformComponent transform, SkinnedMeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable || !mesh.Material.IsAvailable)
            return;

        Material material = mesh.Material.Res!;
        Matrix4x4 localToWorldMatrix = Transform.GetLocalToWorldMatrix(transform);

        GetBoneMatrices(localToWorldMatrix, mesh);
        material.EnableKeyword("SKINNED");
#warning TODO: Set SkinnedMeshRenderer ObjectID

        //material.SetInt("ObjectID", Entity.InstanceID);
        material.SetMatrices("bindPoses", mesh.Mesh.Res!.BindPoses!);
        material.SetMatrices("boneTransforms", mesh.BoneTransforms);

        localToWorldMatrix.Translation -= Camera.RenderingCamera!.Entity.Transform.Position;

        for (int i = 0; i < material.PassCount; i++)
        {
            material.SetPass(i);
            Graphics.DrawMeshNow(mesh.Mesh.Res!, localToWorldMatrix, material);
        }

        material.DisableKeyword("SKINNED");
    }


    private void RenderOpaqueSkinnedMeshDepth(TransformComponent transform, SkinnedMeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable || !mesh.Material.IsAvailable)
            return;

        Material material = mesh.Material.Res!;
        Matrix4x4 localToWorldMatrix = Transform.GetLocalToWorldMatrix(transform);

        GetBoneMatrices(localToWorldMatrix, mesh);
        material.EnableKeyword("SKINNED");
        material.SetMatrices("bindPoses", mesh.Mesh.Res!.BindPoses!);
        material.SetMatrices("boneTransforms", mesh.BoneTransforms);

        localToWorldMatrix.Translation -= Camera.RenderingCamera!.Entity.Transform.Position;

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, localToWorldMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        mesh.Material.Res!.SetMatrix("_MatMVP", mvp);
        mesh.Material.Res!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(mesh.Mesh.Res!);

        material.DisableKeyword("SKINNED");
    }


    private static void GetBoneMatrices(Matrix4x4 localToWorldMatrix, SkinnedMeshRendererComponent mesh)
    {
        mesh.BoneTransforms = new System.Numerics.Matrix4x4[mesh.Bones.Length];
        for (int i = 0; i < mesh.Bones.Length; i++)
        {
            Transform? t = mesh.Bones[i];
            if (t == null)
                mesh.BoneTransforms[i] = System.Numerics.Matrix4x4.Identity;
            else
                mesh.BoneTransforms[i] = (t.LocalToWorldMatrix * localToWorldMatrix.Invert()).ToFloat();
        }
    }

    #endregion


    #region Directional lights

    private void UpdateDirectionalLightShadowmap(TransformComponent transform, DirectionalLightComponent light)
    {
        // Populate the shadowmap
        if (light.CastShadows)
        {
            int res = (int)light.ShadowResolution;
            light.ShadowMap ??= new RenderTexture(res, res, 0);

            // Compute the MVP matrix from the light's point of view
            Graphics.DepthProjectionMatrix = Matrix4x4.CreateOrthographic(light.ShadowDistance, light.ShadowDistance, 0, light.ShadowDistance * 2);

            Quaternion rotation = Transform.GetRotation(transform);
            Vector3 forward = rotation * Vector3.Forward;
            Vector3 up = rotation * Vector3.Up;

            Graphics.DepthViewMatrix = Matrix4x4.CreateLookToLeftHanded(-forward * light.ShadowDistance, -forward, up);

            Matrix4x4 depthMVP = Matrix4x4.Identity;
            depthMVP = Matrix4x4.Multiply(depthMVP, Graphics.DepthViewMatrix);
            depthMVP = Matrix4x4.Multiply(depthMVP, Graphics.DepthProjectionMatrix);
            light.DepthMVP = depthMVP;

            light.ShadowMap.Begin();
            Graphics.Clear(1, 1, 1, 1);

            RenderAllOpaqueMeshesDepth();

            light.ShadowMap.End();
        }
        else
        {
            light.ShadowMap?.DestroyImmediate();
            light.ShadowMap = null;
        }
    }


    private void RenderDirectionalLight(TransformComponent transform, DirectionalLightComponent light)
    {
        Material? lightMat = light.LightMat;
        if (lightMat == null)
        {
            lightMat = new Material(Shader.Find("Defaults/DirectionalLight.shader"));
            light.LightMat = lightMat;
        }

        Color color = light.Color;
        float intensity = light.Intensity;
        bool castShadows = light.CastShadows;
        RenderTexture shadowMap = light.ShadowMap!;
        Matrix4x4 depthMVP = light.DepthMVP;
        float shadowRadius = light.ShadowRadius;
        float shadowPenumbra = light.ShadowPenumbra;
        float shadowMinimumPenumbra = light.ShadowMinimumPenumbra;
        float qualitySamples = light.QualitySamples;
        float blockerSamples = light.BlockerSamples;
        float shadowBias = light.ShadowBias;
        float shadowNormalBias = light.ShadowNormalBias;

        Quaternion rotation = Transform.GetRotation(transform);
        Vector3 forward = rotation * Vector3.Forward;

        lightMat.SetVector("LightDirection", Vector3.TransformNormal(forward, Graphics.ViewMatrix));
        lightMat.SetColor("LightColor", color);
        lightMat.SetFloat("LightIntensity", intensity);

        lightMat.SetTexture("gAlbedoAO", Camera.RenderingCamera!.GBuffer!.AlbedoAO);
        lightMat.SetTexture("gNormalMetallic", Camera.RenderingCamera.GBuffer.NormalMetallic);
        lightMat.SetTexture("gPositionRoughness", Camera.RenderingCamera.GBuffer.PositionRoughness);

        if (castShadows)
        {
            lightMat.EnableKeyword("CASTSHADOWS");
            lightMat.SetTexture("shadowMap", shadowMap.InternalDepth);

            lightMat.SetMatrix("matCamViewInverse", Graphics.InverseViewMatrix);
            lightMat.SetMatrix("matShadowView", Graphics.DepthViewMatrix);
            lightMat.SetMatrix("matShadowSpace", depthMVP);

            lightMat.SetFloat("u_Radius", shadowRadius);
            lightMat.SetFloat("u_Penumbra", shadowPenumbra);
            lightMat.SetFloat("u_MinimumPenumbra", shadowMinimumPenumbra);
            lightMat.SetInt("u_QualitySamples", (int)qualitySamples);
            lightMat.SetInt("u_BlockerSamples", (int)blockerSamples);
            lightMat.SetFloat("u_Bias", shadowBias);
            lightMat.SetFloat("u_NormalBias", shadowNormalBias);
        }
        else
        {
            lightMat.DisableKeyword("CASTSHADOWS");
        }

        Graphics.Blit(lightMat);

        Gizmos.Matrix = Transform.GetLocalToWorldMatrix(transform);
        Gizmos.Color = Color.Yellow;
        Gizmos.DrawDirectionalLight(Vector3.Zero);
    }

    #endregion
}
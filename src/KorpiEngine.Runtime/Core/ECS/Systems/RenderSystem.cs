using Arch.Core;
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
internal class RenderSystem : NativeSystem
{
    private readonly QueryDescription _meshRendererQuery = new QueryDescription().WithAll<TransformComponent, MeshRendererComponent>();
    private readonly QueryDescription _directionalLightQuery = new QueryDescription().WithAll<TransformComponent, DirectionalLightComponent>();


    /// <summary>
    /// Renders everything in the scene.
    /// </summary>
    public RenderSystem(Scene scene) : base(scene)
    {
    }


    protected override void SystemEarlyUpdate(in double deltaTime)
    {
        UpdateAllLightShadowmaps();
    }


    protected override void SystemUpdate(in double deltaTime)
    {
        RenderAllOpaqueMeshes();
        RenderAllDirectionalLights();
    }


    protected override void SystemLateUpdate(in double deltaTime)
    {
    }


    private void UpdateAllLightShadowmaps()
    {
        World.Query(in _directionalLightQuery, (ref TransformComponent t, ref DirectionalLightComponent l) => UpdateLightShadowmap(t, l));
    }


    private void RenderAllOpaqueMeshes()
    {
        World.Query(in _meshRendererQuery, (ref TransformComponent t, ref MeshRendererComponent m) => RenderOpaqueMesh(t, m));
    }
    
    
    private void RenderAllOpaqueMeshesDepth()
    {
        World.Query(in _meshRendererQuery, (ref TransformComponent t, ref MeshRendererComponent m) => RenderOpaqueMeshDepth(t, m));
    }
    
    
    private void RenderAllDirectionalLights()
    {
        World.Query(in _directionalLightQuery, (ref TransformComponent t, ref DirectionalLightComponent l) => RenderDirectionalLight(t, l));
    }


    private void RenderOpaqueMesh(TransformComponent transform, MeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable)
            return;

        Material mat = mesh.Material.Res ?? new Material(Shader.Find("Defaults/Invalid.shader"));

        for (int i = 0; i < mat.PassCount; i++)
        {
            mat.SetPass(i);
            Matrix4x4 matrix = Transform.GetLocalToWorldMatrix(transform);
            Graphics.DrawMeshNow(mesh.Mesh.Res!, matrix, mat);
        }
    }


    private void RenderOpaqueMeshDepth(TransformComponent transform, MeshRendererComponent mesh)
    {
        if (!mesh.Mesh.IsAvailable || !mesh.Material.IsAvailable)
            return;
        
        Matrix4x4 mat = Transform.GetLocalToWorldMatrix(transform);

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, mat);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.DepthProjectionMatrix);
        mesh.Material.Res!.SetMatrix("_MatMVP", mvp);
        mesh.Material.Res!.SetShadowPass(true);
        Graphics.DrawMeshNowDirect(mesh.Mesh.Res!);
    }


    private void UpdateLightShadowmap(TransformComponent transform, DirectionalLightComponent light)
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
}
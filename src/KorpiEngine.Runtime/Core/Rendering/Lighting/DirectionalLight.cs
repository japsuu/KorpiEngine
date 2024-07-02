using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.Rendering.Lighting;

public sealed class DirectionalLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.Lighting;

    public enum Resolution
    {
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }

    public Resolution ShadowResolution = Resolution._1024;

    public Color Color = Color.White;
    public float Intensity = 8f;
    public float QualitySamples = 16;
    public float BlockerSamples = 16;
    public float ShadowDistance = 50f;
    public float ShadowRadius = 0.02f;
    public float ShadowPenumbra = 80f;
    public float ShadowMinimumPenumbra = 0.02f;
    public float ShadowBias = 0.00004f;
    public float ShadowNormalBias = 0.02f;
    public bool CastShadows = true;

    private Material? _lightMat;

    private RenderTexture? _shadowMap;
    private Matrix4x4 _depthMVP;

    protected override void OnPreRender()
    {
        UpdateShadowmap();
    }

    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Defaults/DirectionalLight.shader"));
        _lightMat.SetVector("LightDirection", Vector3.TransformNormal(Entity.Transform.Forward, Graphics.ViewMatrix));
        _lightMat.SetColor("LightColor", Color);
        _lightMat.SetFloat("LightIntensity", Intensity);

        _lightMat.SetTexture("gAlbedoAO", CameraComponent.RenderingCamera.GBuffer!.AlbedoAO);
        _lightMat.SetTexture("gNormalMetallic", CameraComponent.RenderingCamera.GBuffer.NormalMetallic);
        _lightMat.SetTexture("gPositionRoughness", CameraComponent.RenderingCamera.GBuffer.PositionRoughness);

        if (CastShadows)
        {
            _lightMat.EnableKeyword("CASTSHADOWS");
            _lightMat.SetTexture("shadowMap", _shadowMap!.InternalDepth!);

            Matrix4x4.Invert(Graphics.ViewMatrix, out Matrix4x4 viewInverse);

            _lightMat.SetMatrix("matCamViewInverse", viewInverse);
            _lightMat.SetMatrix("matShadowView", Graphics.DepthViewMatrix);
            _lightMat.SetMatrix("matShadowSpace", _depthMVP);

            _lightMat.SetFloat("u_Radius", ShadowRadius);
            _lightMat.SetFloat("u_Penumbra", ShadowPenumbra);
            _lightMat.SetFloat("u_MinimumPenumbra", ShadowMinimumPenumbra);
            _lightMat.SetInt("u_QualitySamples", (int)QualitySamples);
            _lightMat.SetInt("u_BlockerSamples", (int)BlockerSamples);
            _lightMat.SetFloat("u_Bias", ShadowBias);
            _lightMat.SetFloat("u_NormalBias", ShadowNormalBias);
        }
        else
        {
            _lightMat.DisableKeyword("CASTSHADOWS");
        }

        Graphics.Blit(_lightMat);

        /*Gizmos.Matrix = Entity.Transform.LocalToWorldMatrix;
        Gizmos.Color = Color.Yellow;
        Gizmos.DrawDirectionalLight(Vector3.Zero);*/
    }


    private void UpdateShadowmap()
    {
        // Populate Shadowmap
        if (CastShadows)
        {
            int res = (int)ShadowResolution;
            _shadowMap ??= new RenderTexture(res, res, 0);

            // Compute the MVP matrix from the light's point of view
            Graphics.DepthProjectionMatrix = Matrix4x4.CreateOrthographic(ShadowDistance, ShadowDistance, 0, ShadowDistance*2);

            Vector3 forward = Entity.Transform.Forward;
            Graphics.DepthViewMatrix = Matrix4x4.CreateLookToLeftHanded(-forward * ShadowDistance, -forward, Entity.Transform.Up);

            _depthMVP = Matrix4x4.Identity;
            _depthMVP = Matrix4x4.Multiply(_depthMVP, Graphics.DepthViewMatrix);
            _depthMVP = Matrix4x4.Multiply(_depthMVP, Graphics.DepthProjectionMatrix);

            //Graphics.MatDepth = depthMVP;

            _shadowMap.Begin();
            
            Graphics.Clear(1, 1, 1, 1);
            Entity.Scene.EntityScene.InvokeRenderObjectDepthOnAllOfOrder(ComponentRenderOrder.Opaque);
            
            _shadowMap.End();
        }
        else
        {
            _shadowMap?.DestroyImmediate();
            _shadowMap = null;
        }
    }
}
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;

namespace KorpiEngine.Core.Rendering;

public class DirectionalLight : Component
{
    internal override Type NativeComponentType => typeof(DirectionalLightComponent);
    public override RenderingOrder RenderOrder => RenderingOrder.Lighting;

    public override void OnPreRender()
    {
        // Populate Shadowmap
        if (castShadows)
        {
            int res = (int)shadowResolution;
            shadowMap ??= new RenderTexture(res, res, 0);

            // Compute the MVP matrix from the light's point of view
            //Graphics.MatDepthProjection = Matrix4x4.CreateOrthographicOffCenter(-25, 25, -25, 25, 1, 256);
            Graphics.MatDepthProjection = Matrix4x4.CreateOrthographic(shadowDistance, shadowDistance, 0, shadowDistance*2);

            var forward = GameObject.Transform.forward;
            Graphics.MatDepthView = Matrix4x4.CreateLookToLeftHanded(-forward * shadowDistance, -forward, GameObject.Transform.up);

            depthMVP = Matrix4x4.Identity;
            depthMVP = Matrix4x4.Multiply(depthMVP, Graphics.MatDepthView);
            depthMVP = Matrix4x4.Multiply(depthMVP, Graphics.MatDepthProjection);

            //Graphics.MatDepth = depthMVP;

            shadowMap.Begin();
            Graphics.Clear(1, 1, 1, 1);
            foreach (var go in SceneManager.AllGameObjects)
                if (go.enabledInHierarchy)
                    foreach (var comp in go.GetComponents())
                        if (comp.Enabled && comp.RenderOrder == RenderingOrder.Opaque)
                            comp.OnRenderObjectDepth();
            shadowMap.End();
        }
        else
        {
            shadowMap?.DestroyImmediate();
            shadowMap = null;
        }
    }

    public override void OnRenderObject()
    {
        lightMat ??= new Material(Shader.Find("Defaults/Directionallight.shader"));
        lightMat.SetVector("LightDirection", Vector3.TransformNormal(GameObject.Transform.forward, Graphics.MatView));
        lightMat.SetColor("LightColor", color);
        lightMat.SetFloat("LightIntensity", intensity);

        lightMat.SetTexture("gAlbedoAO", Camera.Current.gBuffer.AlbedoAO);
        lightMat.SetTexture("gNormalMetallic", Camera.Current.gBuffer.NormalMetallic);
        lightMat.SetTexture("gPositionRoughness", Camera.Current.gBuffer.PositionRoughness);

        if (castShadows)
        {
            lightMat.EnableKeyword("CASTSHADOWS");
            lightMat.SetTexture("shadowMap", shadowMap.InternalDepth);

            lightMat.SetMatrix("matCamViewInverse", Graphics.MatViewInverse);
            lightMat.SetMatrix("matShadowView", Graphics.MatDepthView);
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

        Gizmos.Matrix = GameObject.Transform.localToWorldMatrix;
        Gizmos.Color = Color.yellow;
        Gizmos.DrawDirectionalLight(Vector3.zero);
    }
}
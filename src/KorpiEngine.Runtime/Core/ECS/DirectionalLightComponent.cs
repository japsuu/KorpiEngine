using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.ECS;

internal struct DirectionalLightComponent() : INativeComponent
{
    public ShadowResolution ShadowResolution = ShadowResolution._1024;
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

    public Matrix4x4 DepthMVP;
    public Material? LightMat;
    public RenderTexture? ShadowMap;
}
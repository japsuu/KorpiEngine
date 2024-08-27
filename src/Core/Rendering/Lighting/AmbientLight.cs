using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.Rendering.Lighting;

public class AmbientLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public Color SkyColor { get; set; } = Color.White;
    public Color GroundColor { get; set; } = Color.White;
    public float SkyIntensity { get; set; } = 0.4f;
    public float GroundIntensity { get; set; } = 0.05f;

    private Material? _lightMat;
    
    
    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Assets/Defaults/AmbientLight.kshader"), "ambient light material", false);

        _lightMat.SetColor("_SkyColor", SkyColor);
        _lightMat.SetColor("_GroundColor", GroundColor);
        _lightMat.SetFloat("_SkyIntensity", SkyIntensity);
        _lightMat.SetFloat("_GroundIntensity", GroundIntensity);

        GBuffer gBuffer = Camera.RenderingCamera.GBuffer!;
        _lightMat.SetTexture("_GAlbedoAO", gBuffer.AlbedoAO);
        _lightMat.SetTexture("_GNormalMetallic", gBuffer.NormalMetallic);
        _lightMat.SetTexture("_GPositionRoughness", gBuffer.PositionRoughness);

        Graphics.Blit(_lightMat);
    }
}
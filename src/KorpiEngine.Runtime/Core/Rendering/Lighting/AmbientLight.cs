using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.Rendering.Lighting;

public class AmbientLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public Color SkyColor = Color.White;
    public Color GroundColor = Color.White;
    public float SkyIntensity = 0.4f;
    public float GroundIntensity = 0.05f;

    private Material? _lightMat;
    
    
    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Defaults/AmbientLight.kshader"), "ambient light material");

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
﻿using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.Rendering.Lighting;

public class AmbientLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.Lighting;

    public Color SkyColor = Color.White;
    public Color GroundColor = Color.White;
    public float SkyIntensity = 1f;
    public float GroundIntensity = 1f;

    private Material? _lightMat;
    
    
    protected override void OnRenderObject()
    {
        _lightMat ??= new Material(Shader.Find("Defaults/AmbientLight.shader"));

        _lightMat.SetColor("SkyColor", SkyColor);
        _lightMat.SetColor("GroundColor", GroundColor);
        _lightMat.SetFloat("SkyIntensity", SkyIntensity);
        _lightMat.SetFloat("GroundIntensity", GroundIntensity);

        GBuffer gBuffer = CameraComponent.RenderingCamera.GBuffer!;
        _lightMat.SetTexture("gAlbedoAO", gBuffer.AlbedoAO);
        _lightMat.SetTexture("gNormalMetallic", gBuffer.NormalMetallic);
        _lightMat.SetTexture("gPositionRoughness", gBuffer.PositionRoughness);

        Graphics.Blit(_lightMat);
    }
}
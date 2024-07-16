using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.Rendering.Lighting;

public class PointLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public Color Color = Color.White;
    public float Radius = 4.0f;
    public float Intensity = 1.0f;

    private Material? _lightMat;
    private Mesh? _mesh;
    private int _lastCamID = -1;

    protected override void OnRenderObject()
    {
        _mesh ??= Mesh.CreateSphere(1f, 16, 16);

        Matrix4x4 mat = Matrix4x4.CreateScale(Radius) * Entity.GlobalCameraRelativeTransform;
        if (_lightMat == null)
        {
            _lightMat = new Material(Shader.Find("Defaults/PointLight.shader"), "point light material");
        }
        else
        {
            if (_lastCamID != CameraComponent.RenderingCamera.InstanceID)
            {
                _lastCamID = CameraComponent.RenderingCamera.InstanceID;
                _lightMat.SetTexture("_GAlbedoAO", CameraComponent.RenderingCamera.GBuffer!.AlbedoAO);
                _lightMat.SetTexture("_GNormalMetallic", CameraComponent.RenderingCamera.GBuffer.NormalMetallic);
                _lightMat.SetTexture("_GPositionRoughness", CameraComponent.RenderingCamera.GBuffer.PositionRoughness);
            }

            _lightMat.SetVector("_LightPosition", Vector3.Transform(Entity.Transform.Position - CameraComponent.RenderingCamera.Entity.Transform.Position, Graphics.ViewMatrix));
            _lightMat.SetColor("_LightColor", Color);
            _lightMat.SetFloat("_LightRadius", Radius);
            _lightMat.SetFloat("_LightIntensity", Intensity);

            _lightMat.SetPass(0);
            
            Graphics.DrawMeshNow(_mesh, mat, _lightMat);
        }

        Gizmos.Matrix = Entity.Transform.LocalToWorldMatrix;
        Gizmos.Color = Color.Yellow;
        Gizmos.DrawSphere(Vector3.Zero, Radius);
    }
}
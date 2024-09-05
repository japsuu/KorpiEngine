using KorpiEngine.EntityModel;
using KorpiEngine.Rendering.Cameras;
using KorpiEngine.Rendering.Materials;
using KorpiEngine.Rendering.Shaders;

namespace KorpiEngine.Rendering.Lighting;

public class PointLight : EntityComponent
{
    public override ComponentRenderOrder RenderOrder => ComponentRenderOrder.LightingPass;

    public ColorHDR Color { get; set; } = ColorHDR.White;
    public float Radius { get; set; } = 4.0f;
    public float Intensity { get; set; } = 1.0f;

    private Material? _lightMat;
    private Mesh? _mesh;
    private int _lastCamID = -1;

    protected override void OnRenderObject()
    {
        _mesh ??= Mesh.CreateSphere(1f, 16, 16);

        Matrix4x4 mat = Matrix4x4.CreateScale(Radius) * Entity.GlobalCameraRelativeTransform;
        if (_lightMat == null)
        {
            _lightMat = new Material(Shader.Find("Assets/Defaults/PointLight.kshader"), "point light material", false);
        }
        else
        {
            if (_lastCamID != Camera.RenderingCamera.InstanceID)
            {
                _lastCamID = Camera.RenderingCamera.InstanceID;
                _lightMat.SetTexture("_GAlbedoAO", Camera.RenderingCamera.GBuffer!.AlbedoAO);
                _lightMat.SetTexture("_GNormalMetallic", Camera.RenderingCamera.GBuffer.NormalMetallic);
                _lightMat.SetTexture("_GPositionRoughness", Camera.RenderingCamera.GBuffer.PositionRoughness);
            }

            _lightMat.SetVector("_LightPosition", MathOps.Transform(Entity.Transform.Position - Camera.RenderingCamera.Entity.Transform.Position, Graphics.ViewMatrix));
            _lightMat.SetColor("_LightColor", Color);
            _lightMat.SetFloat("_LightRadius", Radius);
            _lightMat.SetFloat("_LightIntensity", Intensity);

            _lightMat.SetPass(0);
            
            Graphics.DrawMeshNow(_mesh, mat, _lightMat);
        }

        Gizmos.Matrix = Entity.Transform.LocalToWorldMatrix;
        Gizmos.Color = ColorHDR.Yellow;
        Gizmos.DrawSphere(Vector3.Zero, Radius);
    }
}
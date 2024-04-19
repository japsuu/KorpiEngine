using System.Net.Mime;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

public static class Graphics
{
    public static GraphicsDriver Driver { get; private set; } = null!;


    public static void Initialize(GraphicsDriver driver)
    {
        Driver = driver;
        Driver.Initialize();
    }


    public static void Shutdown()
    {
        Driver.Shutdown();
    }


    public static void DrawMeshNow(Mesh mesh, Matrix4 transform, Material material, Matrix4? oldTransform = null)
    {
        if (Camera.Current == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        if (Graphics.Device.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        oldTransform ??= transform;

        if (defaultNoise.IsAvailable == false)
            defaultNoise = MediaTypeNames.Application.AssetProvider.LoadAsset<Texture2D>("Defaults/noise.png");

        material.SetTexture("DefaultNoise", defaultNoise);

        if (UseJitter)
        {
            material.SetVector("Jitter", Jitter);
            material.SetVector("PreviousJitter", PreviousJitter);
        }
        else
        {
            material.SetVector("Jitter", Vector2.zero);
            material.SetVector("PreviousJitter", Vector2.zero);
        }

        material.SetVector("Resolution", Graphics.Resolution);

        //material.SetVector("ScreenResolution", new Vector2(Window.InternalWindow.FramebufferSize.X, Window.InternalWindow.FramebufferSize.Y));
        material.SetFloat("Time", (float)Time.time);
        material.SetInt("Frame", (int)Time.frameCount);

        //material.SetFloat("DeltaTime", Time.deltaTimeF);
        //material.SetInt("RandomSeed", Random.Shared.Next());
        //material.SetInt("ObjectID", mesh.InstanceID);
        material.SetVector("Camera_WorldPosition", Camera.Current.GameObject.Transform.position);

        //material.SetVector("Camera_NearFarFOV", new Vector3(Camera.Current.NearClip, Camera.Current.FarClip, Camera.Current.FieldOfView));

        // Upload view and projection matrices(if locations available)
        material.SetMatrix("matView", MatView);
        material.SetMatrix("matOldView", OldMatView);

        material.SetMatrix("matProjection", MatProjection);
        material.SetMatrix("matProjectionInverse", MatProjectionInverse);
        material.SetMatrix("matOldProjection", OldMatProjection);

        // Model transformation matrix is sent to shader
        material.SetMatrix("matModel", transform);

        material.SetMatrix("matViewInverse", MatViewInverse);

        Matrix4 matMVP = Matrix4.Identity;
        matMVP = Matrix4.Multiply(matMVP, transform);
        matMVP = Matrix4.Multiply(matMVP, MatView);
        matMVP = Matrix4.Multiply(matMVP, MatProjection);

        Matrix4 oldMatMVP = Matrix4.Identity;
        oldMatMVP = Matrix4.Multiply(oldMatMVP, oldTransform.Value);
        oldMatMVP = Matrix4.Multiply(oldMatMVP, OldMatView);
        oldMatMVP = Matrix4.Multiply(oldMatMVP, OldMatProjection);

        // Send combined model-view-projection matrix to shader
        //material.SetMatrix("mvp", matModelViewProjection);
        material.SetMatrix("mvp", matMVP);
        Matrix4.Invert(matMVP, out var mvpInverse);
        material.SetMatrix("mvpInverse", mvpInverse);
        material.SetMatrix("mvpOld", oldMatMVP);

        // Mesh data can vary between meshes, so we need to let the shaders know which attributes are in use
        material.SetKeyword("HAS_NORMALS", mesh.HasNormals);
        material.SetKeyword("HAS_TANGENTS", mesh.HasTangents);
        material.SetKeyword("HAS_UV", mesh.HasUV);
        material.SetKeyword("HAS_UV2", mesh.HasUV2);
        material.SetKeyword("HAS_COLORS", mesh.HasColors || mesh.HasColors32);

        material.SetKeyword("HAS_BONEINDICES", mesh.HasBoneIndices);
        material.SetKeyword("HAS_BONEWEIGHTS", mesh.HasBoneWeights);

        // All material uniforms have been assigned, its time to properly set them
        MaterialPropertyBlock.Apply(material.PropertyBlock, Graphics.Device.CurrentProgram);

        DrawMeshNowDirect(mesh);
    }


    public static void DrawMeshNowDirect(Mesh mesh)
    {
        if (Camera.Current == null)
            throw new Exception("DrawMeshNow must be called during a rendering context like OnRenderObject()!");
        if (Graphics.Device.CurrentProgram == null)
            throw new Exception("Non Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        mesh.Upload();

        unsafe
        {
            Device.BindVertexArray(mesh.VertexArrayObject);
            Device.DrawElements(Topology.Triangles, (uint)mesh.IndexCount, mesh.IndexFormat == IndexFormat.UInt32, null);
            Device.BindVertexArray(null);
        }
    }


    /// <summary>
    /// Draws material with a FullScreen Quad
    /// </summary>
    public static void Blit(Material mat, int pass = 0)
    {
        mat.SetPass(pass);
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4.Identity, mat);
    }
}
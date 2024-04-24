using System.Net.Mime;
using System.Runtime.CompilerServices;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Rendering.Textures;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

public static class Graphics
{
    private static GraphicsDriver driver = null!;
    private static KorpiWindow Window { get; set; } = null!;
    
    public static Vector2i Resolution { get; private set; } = Vector2i.Zero;
    public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewProjectionMatrix { get; private set; } = Matrix4.Identity;


    internal static void Initialize<T>(KorpiWindow korpiWindow) where T : GraphicsDriver, new()
    {
        driver = new T();
        Window = korpiWindow;
        driver.Initialize();
    }


    internal static void Shutdown()
    {
        driver.Shutdown();
    }
    

    public static void UpdateViewport(int width, int height)
    {
        driver.UpdateViewport(0, 0, width, height);
        Resolution = new Vector2i(width, height);
    }
    

    public static void Clear(float r = 1, float g = 0, float b = 1, float a = 1, bool color = true, bool depth = true, bool stencil = true)
    {
        ClearFlags flags = 0;
        if (color) flags |= ClearFlags.Color;
        if (depth) flags |= ClearFlags.Depth;
        if (stencil) flags |= ClearFlags.Stencil;
        driver.Clear(r, g, b, a, flags);
    }
    
    
    /// <summary>
    /// Starts a new draw frame.
    /// </summary>
    /// <param name="renderingCamera">The camera to render with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartFrame(Camera renderingCamera)
    {
        Camera.RenderingCamera = renderingCamera;
        SetMatrices(renderingCamera);

        if (renderingCamera.ClearType == CameraClearType.Nothing)
            return;
        
        renderingCamera.ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
        
        Clear(r, g, b, a);

        driver.SetState(new RasterizerState(), true);
    }


    /// <summary>
    /// Ends the current draw frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EndFrame()
    {
        Camera.RenderingCamera = null;
    }


    /// <summary>
    /// Called instead of <see cref="StartFrame"/> and <see cref="EndFrame"/> when there is no camera to render with.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipFrame()
    {
        Clear();
    }


    public static void DrawMeshNow(Mesh mesh, Matrix4 transform, Material material, Matrix4? oldTransform = null)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (driver.CurrentProgram == null)
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
    
    
    /*public static void RenderMesh(Mesh mesh, Material material, Matrix4 modelMatrix)
    {
        material.SetModelMatrix(modelMatrix);
        material.SetViewMatrix(ViewMatrix);
        material.SetProjectionMatrix(ProjectionMatrix);
        
        material.Bind();
        
        mesh.UploadMeshData();
    }*/


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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetMatrices(Camera renderingCamera)
    {
        Matrix4 viewMatrix = renderingCamera.ViewMatrix;
        Matrix4 projectionMatrix = renderingCamera.ProjectionMatrix;
        
        ProjectionMatrix = projectionMatrix;
        ViewMatrix = viewMatrix;
        ViewProjectionMatrix = viewMatrix * projectionMatrix;
    }
}
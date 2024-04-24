using System.Runtime.CompilerServices;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.Rendering.Primitives;
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


    public static void DrawMeshNow(Mesh mesh, Matrix4 transform, Material material)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (driver.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        // Upload the default uniforms available to all shaders.
        // The shader can choose to use them or not, as they are buffered only if the location is available.
        material.SetVector("u_Resolution", Resolution);
        material.SetFloat("u_Time", (float)Time.TotalTime);
        material.SetInt("u_Frame", Time.TotalFrameCount);
        
        // Camera data
        material.SetVector("u_Camera_WorldPosition", Camera.RenderingCamera.Transform.Position);
        material.SetVector("u_Camera_Forward", Camera.RenderingCamera.Transform.Forward);
        
        // Matrices
        Matrix4 matMVP = Matrix4.Identity * transform * ViewMatrix * ProjectionMatrix;
        material.SetMatrix("u_MatMVP", matMVP);
        material.SetMatrix("u_MatModel", transform);
        material.SetMatrix("u_MatView", ViewMatrix);
        material.SetMatrix("u_MatProjection", ProjectionMatrix);

        // Mesh data can vary from mesh to mesh, so we need to let the shader know which attributes are currently in use
        material.SetKeyword("HAS_UV", mesh.HasUV);
        material.SetKeyword("HAS_UV2", mesh.HasUV2);
        material.SetKeyword("HAS_NORMALS", mesh.HasNormals);
        material.SetKeyword("HAS_COLORS", mesh.HasColors || mesh.HasColors32);
        material.SetKeyword("HAS_TANGENTS", mesh.HasTangents);

        // All material uniforms have been assigned, it's time to buffer them
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
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (driver.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        mesh.UploadMeshData();

        unsafe
        {
            driver.BindVertexArray(mesh.VertexArrayObject);
            driver.DrawElements(Topology.Triangles, mesh.IndexCount, mesh.IndexFormat == IndexFormat.UInt32, null);
            driver.BindVertexArray(null);
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
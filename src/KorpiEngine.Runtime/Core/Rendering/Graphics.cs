using System.Diagnostics;
using System.Runtime.CompilerServices;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Windowing;

namespace KorpiEngine.Core.Rendering;

public static class Graphics
{
    private static KorpiWindow Window { get; set; } = null!;
    private static Material? defaultMat;
    internal static GraphicsDriver Driver = null!;
    internal static Vector2i FrameBufferSize;

    public static Vector2 Resolution { get; private set; } = Vector2.Zero;
    public static Matrix4x4 ProjectionMatrix { get; private set; } = Matrix4x4.Identity;
    public static Matrix4x4 ViewMatrix { get; private set; } = Matrix4x4.Identity;
    public static Matrix4x4 ViewProjectionMatrix { get; private set; } = Matrix4x4.Identity;


    internal static void Initialize<T>(KorpiWindow korpiWindow) where T : GraphicsDriver, new()
    {
        Driver = new T();
        Window = korpiWindow;
        Driver.Initialize();
    }


    internal static void Shutdown()
    {
        Driver.Shutdown();
    }
    

    internal static void UpdateViewport(int width, int height)
    {
        Driver.UpdateViewport(0, 0, width, height);
        Resolution = new Vector2(width, height);
    }
    

    internal static void Clear(float r = 1, float g = 0, float b = 1, float a = 1, bool color = true, bool depth = true, bool stencil = true)
    {
        ClearFlags flags = 0;
        if (color) flags |= ClearFlags.Color;
        if (depth) flags |= ClearFlags.Depth;
        if (stencil) flags |= ClearFlags.Stencil;
        Driver.Clear(r, g, b, a, flags);
    }
    
    
    /// <summary>
    /// Starts a new draw frame.
    /// </summary>
    /// <param name="renderingCamera">The camera to render with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartFrame(Camera renderingCamera)
    {
        RenderTexture.UpdatePool();

        Camera.RenderingCamera = renderingCamera;
        SetMatrices(renderingCamera);

        if (renderingCamera.ClearType == CameraClearType.Nothing)
            return;
        
        renderingCamera.ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
        Clear(r, g, b, a);

        Driver.SetState(new RasterizerState(), true);
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
    internal static void SkipFrame()
    {
        Clear();
    }


    public static void DrawMeshNow(Mesh mesh, Matrix4x4 transform, Material material)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (Driver.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        // Upload the default uniforms available to all shaders.
        // The shader can choose to use them or not, as they are buffered only if the location is available.
        material.SetVector("_Resolution", Resolution);
        material.SetFloat("_Time", (float)Time.TotalTime);
        material.SetInt("_Frame", Time.TotalFrameCount);
        
        // Camera data
        material.SetVector("_Camera_WorldPosition", Camera.RenderingCamera.Transform.Position);
        material.SetVector("_Camera_Forward", Camera.RenderingCamera.Transform.Forward);
        
        // Matrices
        Matrix4x4 matMVP = Matrix4x4.Identity * transform * ViewMatrix * ProjectionMatrix;
        material.SetMatrix("_MatMVP", matMVP);
        material.SetMatrix("_MatModel", transform);
        material.SetMatrix("_MatView", ViewMatrix);
        material.SetMatrix("_MatProjection", ProjectionMatrix);

        // Mesh data can vary from mesh to mesh, so we need to let the shader know which attributes are currently in use
        material.SetKeyword("HAS_UV", mesh.HasUV0);
        material.SetKeyword("HAS_UV2", mesh.HasUV1);
        material.SetKeyword("HAS_NORMALS", mesh.HasNormals);
        material.SetKeyword("HAS_COLORS", mesh.HasColors || mesh.HasColors32);
        material.SetKeyword("HAS_TANGENTS", mesh.HasTangents);

        material.SetKeyword("HAS_BONEINDICES", mesh.HasBoneIndices);
        material.SetKeyword("HAS_BONEWEIGHTS", mesh.HasBoneWeights);

        // All material uniforms have been assigned, it's time to buffer them
        material.PropertyBlock.Apply(Driver.CurrentProgram);

        DrawMeshNowDirect(mesh);
    }


    public static void DrawMeshNowDirect(Mesh mesh)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (Driver.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        mesh.UploadMeshData();

        unsafe
        {
            Driver.BindVertexArray(mesh.VertexArrayObject);
            Driver.DrawElements(mesh.Topology, mesh.IndexCount, mesh.IndexFormat == IndexFormat.UInt32, (void*)0);
            Driver.BindVertexArray(null);
        }
    }


    /// <summary>
    /// Draws material with a FullScreen Quad
    /// </summary>
    public static void Blit(Material mat, int pass = 0)
    {
        mat.SetPass(pass);
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4x4.Identity, mat);
    }
    
    
    /// <summary>
    /// Draws material with a FullScreen Quad onto a RenderTexture
    /// </summary>
    public static void Blit(RenderTexture? renderTexture, Material mat, int pass = 0, bool clear = true)
    {
        renderTexture?.Begin();
        if (clear)
            Clear(0, 0, 0, 0);
        mat.SetPass(pass);
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4x4.Identity, mat);
        renderTexture?.End();

    }

    /// <summary>
    /// Draws texture into a RenderTexture Additively
    /// </summary>
    public static void Blit(RenderTexture? renderTexture, Texture2D texture, bool clear = true)
    {
        defaultMat ??= new Material(Shader.Find("Defaults/Basic.shader"));
        defaultMat.SetTexture("texture0", texture);
        defaultMat.SetPass(0);

        renderTexture?.Begin();
        if (clear) Clear(0, 0, 0, 0);
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4x4.Identity, defaultMat);
        renderTexture?.End();
    }

    
    internal static void BlitDepth(RenderTexture source, RenderTexture? destination)
    {
        Debug.Assert(source.FrameBuffer != null, "source.FrameBuffer != null");
        Driver.BindFramebuffer(source.FrameBuffer, FBOTarget.ReadFramebuffer);
        if(destination != null)
        {
            Debug.Assert(destination.FrameBuffer != null, "destination.FrameBuffer != null");
            Driver.BindFramebuffer(destination.FrameBuffer, FBOTarget.DrawFramebuffer);
        }

        Driver.BlitFramebuffer(0, 0, source.Width, source.Height,
            0, 0, destination?.Width ?? (int)Resolution.X, destination?.Height ?? (int)Resolution.Y,
            ClearFlags.Depth, BlitFilter.Nearest
        );
        Driver.UnbindFramebuffer();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetMatrices(Camera renderingCamera)
    {
        Matrix4x4 viewMatrix = renderingCamera.ViewMatrix;
        Matrix4x4 projectionMatrix = renderingCamera.ProjectionMatrix;
        
        ProjectionMatrix = projectionMatrix;
        ViewMatrix = viewMatrix;
        ViewProjectionMatrix = viewMatrix * projectionMatrix;
    }
}
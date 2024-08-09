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
    private static Material defaultBlitMaterial = null!;
    
    internal static KorpiWindow Window { get; private set; } = null!;
    internal static GraphicsDevice Device = null!;
    internal static Vector2i FrameBufferSize;
    
    public static Vector2 Resolution { get; private set; } = Vector2.Zero;
    
    public static Matrix4x4 ViewMatrix = Matrix4x4.Identity;
    public static Matrix4x4 OldViewMatrix = Matrix4x4.Identity;
    public static Matrix4x4 InverseViewMatrix = Matrix4x4.Identity;
    public static Matrix4x4 ProjectionMatrix = Matrix4x4.Identity;
    public static Matrix4x4 OldProjectionMatrix = Matrix4x4.Identity;
    public static Matrix4x4 InverseProjectionMatrix = Matrix4x4.Identity;
    
    public static Matrix4x4 DepthProjectionMatrix;
    public static Matrix4x4 DepthViewMatrix;

    public static bool UseJitter;
    public static Vector2 Jitter { get; set; }
    public static Vector2 PreviousJitter { get; set; }
    

    internal static void Initialize<T>(KorpiWindow korpiWindow) where T : GraphicsDevice, new()
    {
        Device = new T();
        Window = korpiWindow;
        defaultBlitMaterial = new Material(Shader.Find("Defaults/Basic.kshader"), "basic material");
        Device.Initialize();
    }


    internal static void Shutdown()
    {
        Device.Shutdown();
    }
    

    internal static void UpdateViewport(int width, int height)
    {
        Device.UpdateViewport(0, 0, width, height);
        Resolution = new Vector2(width, height);
    }
    

    internal static void Clear(float r = 0, float g = 0, float b = 0, float a = 1, bool color = true, bool depth = true, bool stencil = true)
    {
        ClearFlags flags = 0;
        if (color) flags |= ClearFlags.Color;
        if (depth) flags |= ClearFlags.Depth;
        if (stencil) flags |= ClearFlags.Stencil;
        Device.Clear(r, g, b, a, flags);
    }


    /// <summary>
    /// Starts a new draw frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartFrame()
    {
        RenderTexture.UpdatePool();
        
        Clear();
        UpdateViewport(Window.FramebufferSize.X, Window.FramebufferSize.Y);

        Device.SetState(new RasterizerState(), true);
    }


    /// <summary>
    /// Ends the current draw frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EndFrame()
    {
        
    }


    /// <summary>
    /// Draws a mesh with a specified material and transform.
    /// </summary>
    /// <param name="mesh">The mesh to draw.</param>
    /// <param name="camRelativeTransform">A matrix relative/local to the currently rendering camera.</param>
    /// <param name="material">The material to use for rendering.</param>
    /// <param name="oldCamRelativeTransform">The previous frame's camera-relative transform.</param>
    /// <exception cref="Exception">Thrown when DrawMeshNow is called outside a rendering context.</exception>
    public static void DrawMeshNow(Mesh mesh, Matrix4x4 camRelativeTransform, Material material, Matrix4x4? oldCamRelativeTransform = null)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (Device.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");
        
        oldCamRelativeTransform ??= camRelativeTransform;

        // Upload the default uniforms available to all shaders.
        // The shader can choose to use them or not, as they are buffered only if the location is available.
        
        if (UseJitter)
        {
            material.SetVector("_Jitter", Jitter, true);
            material.SetVector("_PreviousJitter", PreviousJitter, true);
        }
        else
        {
            material.SetVector("_Jitter", Vector2.Zero, true);
            material.SetVector("_PreviousJitter", Vector2.Zero, true);
        }
        
        material.SetVector("_Resolution", Resolution, true);
        material.SetFloat("_Time", (float)Time.TotalTime, true);
        material.SetInt("_Frame", Time.TotalFrameCount, true);
        
        // Camera data
        material.SetVector("_Camera_WorldPosition", Camera.RenderingCamera.Transform.Position, true);
        material.SetVector("_Camera_Forward", Camera.RenderingCamera.Transform.Forward, true);
        
        // Matrices
        material.SetMatrix("_MatModel", camRelativeTransform, true);
        material.SetMatrix("_MatView", ViewMatrix, true);
        material.SetMatrix("_MatViewInverse", InverseViewMatrix, true);
        material.SetMatrix("_MatProjection", ProjectionMatrix, true);
        material.SetMatrix("_MatProjectionInverse", InverseProjectionMatrix, true);
        
        Matrix4x4 matMVP = Matrix4x4.Identity;
        matMVP = Matrix4x4.Multiply(matMVP, camRelativeTransform);
        matMVP = Matrix4x4.Multiply(matMVP, ViewMatrix);
        matMVP = Matrix4x4.Multiply(matMVP, ProjectionMatrix);
        material.SetMatrix("_MatMVP", matMVP, true);

        Matrix4x4 oldMatMVP = Matrix4x4.Identity;
        oldMatMVP = Matrix4x4.Multiply(oldMatMVP, oldCamRelativeTransform.Value);
        oldMatMVP = Matrix4x4.Multiply(oldMatMVP, OldViewMatrix);
        oldMatMVP = Matrix4x4.Multiply(oldMatMVP, OldProjectionMatrix);
        material.SetMatrix("_MatMVPOld", oldMatMVP, true);
        
        Matrix4x4.Invert(matMVP, out Matrix4x4 matMVPInverse);
        material.SetMatrix("_MatMVPInverse", matMVPInverse, true);

        // Mesh data can vary from mesh to mesh, so we need to let the shader know which attributes are currently in use
        material.SetKeyword("HAS_UV", mesh.HasVertexUV0);
        material.SetKeyword("HAS_UV2", mesh.HasVertexUV1);
        material.SetKeyword("HAS_NORMALS", mesh.HasVertexNormals);
        material.SetKeyword("HAS_COLORS", mesh.HasVertexColors);
        material.SetKeyword("HAS_TANGENTS", mesh.HasVertexTangents);

        // All material uniforms have been assigned; it's time to buffer them
        material.ApplyPropertyBlock(Device.CurrentProgram);

        DrawMeshNowDirect(mesh);
    }


    public static void DrawMeshNowDirect(Mesh mesh)
    {
        if (Camera.RenderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (Device.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        mesh.UploadMeshData();

        unsafe
        {
            Device.BindVertexArray(mesh.VertexArrayObject);
            Device.DrawElements(mesh.Topology, mesh.IndexCount, mesh.IndexFormat == IndexFormat.UInt32, 0);
            Device.BindVertexArray(null);
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
        defaultBlitMaterial.SetTexture("_Texture0", texture);
        defaultBlitMaterial.SetPass(0);

        renderTexture?.Begin();
        
        if (clear)
            Clear(0, 0, 0, 0);
        
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4x4.Identity, defaultBlitMaterial);
        
        renderTexture?.End();
    }

    
    /// <summary>
    /// Blits the depth buffer from one render texture to another.
    /// </summary>
    /// <param name="source">The source render texture.</param>
    /// <param name="destination">The destination render texture.</param>
    internal static void BlitDepth(RenderTexture source, RenderTexture? destination)
    {
        Device.BindFramebuffer(source.FrameBuffer!, FBOTarget.ReadFramebuffer);
        
        if(destination != null)
            Device.BindFramebuffer(destination.FrameBuffer!, FBOTarget.DrawFramebuffer);

        Device.BlitFramebuffer(0, 0, source.Width, source.Height,
            0, 0, destination?.Width ?? (int)Resolution.X, destination?.Height ?? (int)Resolution.Y,
            ClearFlags.Depth, BlitFilter.Nearest
        );
        Device.UnbindFramebuffer();
    }
}
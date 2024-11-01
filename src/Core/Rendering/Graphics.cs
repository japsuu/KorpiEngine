﻿using System.Runtime.CompilerServices;
using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

public static class Graphics
{
    private static Material defaultBlitMaterial = null!;
    
    internal static GraphicsWindow Window { get; private set; } = null!;
    internal static GraphicsDevice Device { get; private set; } = null!;
    
    public static bool UseJitter { get; set; }
    public static Vector2 Jitter { get; set; }
    public static Vector2 PreviousJitter { get; set; }
    public static Vector2 ViewportResolution { get; private set; } = Vector2.Zero;
    
    public static Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
    public static Matrix4x4 OldViewMatrix { get; set; } = Matrix4x4.Identity;
    public static Matrix4x4 InverseViewMatrix { get; set; } = Matrix4x4.Identity;
    public static Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
    public static Matrix4x4 OldProjectionMatrix { get; set; } = Matrix4x4.Identity;
    public static Matrix4x4 InverseProjectionMatrix { get; set; } = Matrix4x4.Identity;
    
    public static Matrix4x4 DepthProjectionMatrix { get; set; }
    public static Matrix4x4 DepthViewMatrix { get; set; }
    

    internal static void Initialize(GraphicsContext graphicsContext)
    {
        Device = graphicsContext.Device;
        Window = graphicsContext.Window;
        Device.Initialize();
        GraphicsInfo.Initialize(graphicsContext);
        
        defaultBlitMaterial = new Material(Asset.Load<Shader>("Assets/Defaults/Basic.kshader"), "basic material", false);
        Material.LoadDefaults();
    }


    internal static void Shutdown()
    {
        Device.Shutdown();
    }
    

    internal static void UpdateViewport(int width, int height)
    {
        Debug.AssertMainThread(true);

        Device.UpdateViewport(0, 0, width, height);
        ViewportResolution = new Vector2(width, height);
    }
    

    internal static void Clear(float r = 0, float g = 0, float b = 0, float a = 1, bool color = true, bool depth = true, bool stencil = true)
    {
        Debug.AssertMainThread(true);

        ClearFlags flags = 0;
        if (color) flags |= ClearFlags.Color;
        if (depth) flags |= ClearFlags.Depth;
        if (stencil) flags |= ClearFlags.Stencil;
        Device.Clear(r, g, b, a, flags);
    }


    /// <summary>
    /// Starts a new draw frame.
    /// </summary>
    [ProfileInternal]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartFrame()
    {
        Debug.AssertMainThread(true);

        RenderTexture.UpdatePool();
        
        Clear();
        UpdateViewport(Window.FramebufferSize.X, Window.FramebufferSize.Y);

        Device.SetState(new RasterizerState(), true);
#if KORPI_TOOLS
        Device.ResetStatistics();
#endif
    }


    /// <summary>
    /// Ends the current draw frame.
    /// </summary>
    [ProfileInternal]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EndFrame()
    {
        Debug.AssertMainThread(true);

        // Additional functionality before the frame ends
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
        Debug.AssertMainThread(true);

        if (Camera.RenderingCamera == null)
            throw new RenderStateException("DrawMeshNow must be called during a rendering context!");
        
        if (Device.CurrentProgram == null)
            throw new RenderStateException("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");
        
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
        
        material.SetVector("_Resolution", ViewportResolution, true);
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

        material.SetKeyword("HAS_BONEWEIGHTS", mesh.HasBoneWeights);
        material.SetKeyword("HAS_BONEINDICES", mesh.HasBoneIndices);

        // All material uniforms have been assigned; it's time to buffer them
        material.ApplyPropertyBlock(Device.CurrentProgram);

        DrawMeshNowDirect(mesh);
    }


    public static void DrawMeshNowDirect(Mesh mesh)
    {
        Debug.AssertMainThread(true);

        if (Camera.RenderingCamera == null)
            throw new RenderStateException("DrawMeshNow must be called during a rendering context!");
        
        if (Device.CurrentProgram == null)
            throw new RenderStateException("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        mesh.UploadMeshData();

        Device.BindVertexArray(mesh.VertexArrayObject);
        Device.DrawElements(mesh.Topology, 0, mesh.IndexCount, mesh.IndexFormat == IndexFormat.UInt32);
        Device.BindVertexArray(null);
    }
    
    
    public static bool FrustumTest(Sphere boundingSphere, Matrix4x4 transform)
    {
        Debug.AssertMainThread(true);

        if (Camera.RenderingCamera == null)
            throw new RenderStateException("FrustumTest must be called during a rendering context!");
        
        BoundingFrustum frustum = Camera.RenderingCamera.CalculateFrustum();
        ContainmentType type = frustum.Contains(boundingSphere.Transform(transform));
        
        return type != ContainmentType.Disjoint;
    }


    /// <summary>
    /// Draws material with a FullScreen Quad
    /// </summary>
    public static void Blit(Material mat, int pass = 0)
    {
        Debug.AssertMainThread(true);

        mat.SetPass(pass);
        DrawMeshNow(Mesh.GetFullscreenQuad(), Matrix4x4.Identity, mat);
    }

    /// <summary>
    /// Draws material with a FullScreen Quad onto a RenderTexture
    /// </summary>
    public static void Blit(RenderTexture? renderTexture, Material mat, int pass = 0, bool clear = true)
    {
        Debug.AssertMainThread(true);

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
        Debug.AssertMainThread(true);

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
        Debug.AssertMainThread(true);

        Device.BindFramebuffer(source.FrameBuffer!, FBOTarget.ReadFramebuffer);
        
        if(destination != null)
            Device.BindFramebuffer(destination.FrameBuffer!, FBOTarget.DrawFramebuffer);

        Device.BlitFramebuffer(0, 0, source.Width, source.Height,
            0, 0, destination?.Width ?? (int)ViewportResolution.X, destination?.Height ?? (int)ViewportResolution.Y,
            ClearFlags.Depth, BlitFilter.Nearest
        );
        Device.UnbindFramebuffer();
    }
}
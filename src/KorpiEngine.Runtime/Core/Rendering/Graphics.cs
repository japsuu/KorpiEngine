﻿using System.Runtime.CompilerServices;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Windowing;

namespace KorpiEngine.Core.Rendering;

public static class Graphics
{
    private static KorpiWindow Window { get; set; } = null!;
    private static CameraComponent? renderingCamera;
    internal static GraphicsDriver Driver = null!;
    
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartFrame()
    {
        Driver.SetState(new RasterizerState(), true);
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
    /// <param name="transform">The transform to use for rendering.</param>
    /// <param name="material">The material to use for rendering.</param>
    /// <exception cref="Exception">Thrown when DrawMeshNow is called outside a rendering context.</exception>
    public static void DrawMeshNow(Mesh mesh, Transform transform, Material material)
    {
        if (renderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        Matrix4x4 camRelative = transform.LocalToWorldMatrix;
        camRelative.Translation -= renderingCamera.Transform.Position;
        
        DrawMeshNow(mesh, camRelative, material);
    }
    
    
    /// <summary>
    /// Draws a mesh with a specified material and transform.
    /// </summary>
    /// <param name="mesh">The mesh to draw.</param>
    /// <param name="camRelativeTransform">A matrix relative/local to the currently rendering camera.</param>
    /// <param name="material">The material to use for rendering.</param>
    /// <exception cref="Exception">Thrown when DrawMeshNow is called outside a rendering context.</exception>
    public static void DrawMeshNow(Mesh mesh, Matrix4x4 camRelativeTransform, Material material)
    {
        if (renderingCamera == null)
            throw new Exception("DrawMeshNow must be called during a rendering context!");
        
        if (Driver.CurrentProgram == null)
            throw new Exception("No Program Assigned, Use Material.SetPass first before calling DrawMeshNow!");

        // Upload the default uniforms available to all shaders.
        // The shader can choose to use them or not, as they are buffered only if the location is available.
        material.SetVector("u_Resolution", Resolution);
        material.SetFloat("u_Time", (float)Time.TotalTime);
        material.SetInt("u_Frame", Time.TotalFrameCount);
        
        // Camera data
        material.SetVector("u_Camera_WorldPosition", renderingCamera.Transform.Position);
        material.SetVector("u_Camera_Forward", renderingCamera.Transform.Forward);
        
        // Matrices
        Matrix4x4 matMVP = Matrix4x4.Identity * camRelativeTransform * ViewMatrix * ProjectionMatrix;
        material.SetMatrix("u_MatMVP", matMVP);
        material.SetMatrix("u_MatModel", camRelativeTransform);
        material.SetMatrix("u_MatView", ViewMatrix);
        material.SetMatrix("u_MatProjection", ProjectionMatrix);

        // Mesh data can vary from mesh to mesh, so we need to let the shader know which attributes are currently in use
        material.SetKeyword("HAS_UV", mesh.HasUV0);
        material.SetKeyword("HAS_UV2", mesh.HasUV1);
        material.SetKeyword("HAS_NORMALS", mesh.HasNormals);
        material.SetKeyword("HAS_COLORS", mesh.HasColors);
        material.SetKeyword("HAS_TANGENTS", mesh.HasTangents);

        // All material uniforms have been assigned, it's time to buffer them
        material.PropertyBlock.Apply(Driver.CurrentProgram);

        DrawMeshNowDirect(mesh);
    }


    public static void DrawMeshNowDirect(Mesh mesh)
    {
        if (renderingCamera == null)
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetRenderingCamera(CameraComponent? camera)
    {
        renderingCamera = camera;
        
        if (camera == null)
            return;
        
        Matrix4x4 viewMatrix = camera.ViewMatrix;
        Matrix4x4 projectionMatrix = camera.ProjectionMatrix;
        
        ProjectionMatrix = projectionMatrix;
        ViewMatrix = viewMatrix;
        ViewProjectionMatrix = viewMatrix * projectionMatrix;
    }
}
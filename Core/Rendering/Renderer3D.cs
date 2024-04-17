using System.Runtime.CompilerServices;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Windowing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

internal static class Renderer3D
{
    private static readonly Color DefaultClearColor = Color.Magenta;
    
    public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;
    public static Matrix4 ViewProjectionMatrix { get; private set; } = Matrix4.Identity;
    
    
    public static void Initialize()
    {
        Graphics.Driver.Enable(EnableCap.DepthTest);
        
        Graphics.Driver.SetClearColor(DefaultClearColor);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnWindowResize(int width, int height)
    {
        Graphics.Driver.UpdateViewport(0, 0, width, height);
    }
    
    
    /// <summary>
    /// Starts a new draw frame.
    /// </summary>
    /// <param name="renderingCamera">The camera to render with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartFrame(Camera renderingCamera)
    {
        SetMatrices(renderingCamera);

        if (renderingCamera.ClearType == CameraClearType.Nothing)
            return;
        
        Clear(renderingCamera.ClearColor);
    }


    /// <summary>
    /// Ends the current draw frame.
    /// </summary>
    /// <param name="korpiWindow">The window into which the frame is drawn.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndFrame(KorpiWindow korpiWindow)
    {
        korpiWindow.SwapBuffers();
    }


    /// <summary>
    /// Called instead of <see cref="StartFrame"/> and <see cref="EndFrame"/> when there is no camera to render with.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipFrame()
    {
        Clear(DefaultClearColor);
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Clear(Color color)
    {
        Graphics.Driver.SetClearColor(color.R, color.G, color.B, color.A);

        Graphics.Driver.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
}
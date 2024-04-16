using System.Runtime.CompilerServices;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Scripting.Components;
using KorpiEngine.Core.Windowing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

internal static class Renderer3D
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Renderer3D));
#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif
    
    public static RenderCamera RenderCamera = null!;
    
    
    public static void Initialize()
    {
        RenderCamera = new RenderCamera();
#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        GL.Enable(EnableCap.DepthTest);
        // GL.Enable(EnableCap.Multisample);
        GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnWindowResize(int width, int height)
    {
        GL.Viewport(0, 0, width, height);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartFrame(in Matrix4 projectionMatrix, Transform cameraTransform)
    {
        // We need to invert the camera's transform to get the view matrix.
        Matrix4 viewMatrix = cameraTransform.Matrix.Inverted();
        
        RenderCamera.ProjectionMatrix = projectionMatrix;
        RenderCamera.ViewMatrix = viewMatrix;
        RenderCamera.ViewProjectionMatrix = viewMatrix * projectionMatrix;
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndFrame(KorpiWindow korpiWindow)
    {
        korpiWindow.SwapBuffers();
    }
    
    
    public static void DrawQuad(Matrix4 transform, Color color)
    {
        
    }


#if DEBUG
    private static void OnDebugMessage(
        DebugSource source, // Source of the debugging message.
        DebugType type, // Type of the debugging message.
        int id, // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length, // Length of the string in pMessage.
        IntPtr pMessage, // Pointer to message string.
        IntPtr pUserParam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification)
            return;
        
        // In order to access the string pointed to by pMessage, you can use Marshal
        // class to copy its contents to a C# string without unsafe code. You can
        // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
        string message = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(pMessage, length);
        
        Logger.OpenGl($"[{severity} source={source} type={type} id={id}] {message}");

        if (type == DebugType.DebugTypeError)
            throw new Exception(message);
    }
#endif
}
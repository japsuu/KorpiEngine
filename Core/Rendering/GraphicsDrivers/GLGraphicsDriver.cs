using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.GraphicsDrivers;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
public class GLGraphicsDriver : GraphicsDriver
{
#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif
    
    protected override void InitializeInternal()
    {
#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif
    }


    protected override void ShutdownInternal() { }

    
    public override void SetClearColor(float r, float g, float b, float a)
    {
        GL.ClearColor(r, g, b, a);
    }


    public override void SetClearColor(Color color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
    }


    public override void UpdateViewport(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }


    public override void Clear(ClearBufferMask mask)
    {
        GL.Clear(mask);
    }


    public override void Enable(EnableCap mask)
    {
        GL.Enable(mask);
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
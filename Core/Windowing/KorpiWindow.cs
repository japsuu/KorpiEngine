using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Platform;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.Windowing;

public sealed class KorpiWindow : GameWindow
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(KorpiWindow));
#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif
    
    public KorpiWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }


    protected override void OnLoad()
    {
        SystemInfo.Initialize();
        WindowInfo.Initialize(this);
        InputManagement.Cursor.Initialize(this);
        InputManagement.Cursor.SetGrabbed(false);

#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        GL.Enable(EnableCap.DepthTest);     // Enable depth testing.
        // GL.Enable(EnableCap.Multisample);   // Enable multisampling.
        GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
        
        base.OnLoad();
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
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
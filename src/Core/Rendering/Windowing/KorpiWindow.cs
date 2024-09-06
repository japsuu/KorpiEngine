using KorpiEngine.Rendering.OpenGL;
using KorpiEngine.Utils;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Rendering;

internal sealed class KorpiWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    protected override void OnLoad()
    {
        SystemInfo.ProcessorCount = Environment.ProcessorCount;
        SystemInfo.MainThreadId = Environment.CurrentManagedThreadId;
        WindowInfo.Initialize(this);
        Input.Cursor.Initialize(this);

        Graphics.Initialize<GLGraphicsDevice>(this);
        
        base.OnLoad();
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        
        Graphics.Shutdown();
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Graphics.StartFrame();
        base.OnRenderFrame(args);
        Graphics.EndFrame();
        
        SwapBuffers();
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        Graphics.UpdateViewport(e.Width, e.Height);
        
        base.OnResize(e);
    }
}
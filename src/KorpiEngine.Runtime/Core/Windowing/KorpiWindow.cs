using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.Windowing;

internal sealed class KorpiWindow : GameWindow
{
    public KorpiWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }


    protected override void OnLoad()
    {
        SystemInfo.ProcessorCount = Environment.ProcessorCount;
        SystemInfo.MainThreadId = Environment.CurrentManagedThreadId;
        WindowInfo.Initialize(this);
        Core.API.InputManagement.Cursor.Initialize(this);

        Graphics.Initialize<GLGraphicsDriver>(this);
        
        base.OnLoad();
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        
        Graphics.Shutdown();
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Camera? mainCamera = Camera.MainCamera;
        if (mainCamera == null)
        {
            Graphics.SkipFrame();
        }
        else
        {
            Graphics.StartFrame(mainCamera);
            base.OnRenderFrame(args);
            Graphics.EndFrame();
        }
        
        SwapBuffers();
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        Graphics.UpdateViewport(e.Width, e.Height);
        
        base.OnResize(e);
    }
}
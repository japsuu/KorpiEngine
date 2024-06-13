using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace KorpiEngine.Core.Windowing;

internal sealed class KorpiWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : GameWindow(gameWindowSettings, nativeWindowSettings)
{
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


    protected override void OnResize(ResizeEventArgs e)
    {
        Graphics.UpdateViewport(e.Width, e.Height);
        
        base.OnResize(e);
    }
}
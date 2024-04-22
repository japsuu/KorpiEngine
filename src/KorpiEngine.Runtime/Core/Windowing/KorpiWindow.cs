﻿using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
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
        InputManagement.Cursor.Initialize(this);
        InputManagement.Cursor.SetGrabbed(false);

        Renderer3D.Initialize();
        
        base.OnLoad();
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Camera? mainCamera = Camera.MainCamera;
        if (mainCamera == null)
        {
            Renderer3D.SkipFrame();
            return;
        }
        
        Renderer3D.StartFrame(mainCamera);
        base.OnRenderFrame(args);
        Renderer3D.EndFrame(this);
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        Renderer3D.OnWindowResize(e.Width, e.Height);
        
        base.OnResize(e);
    }
}
﻿using KorpiEngine.Core.Debugging.Profiling;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Threading.Pooling;
using KorpiEngine.Core.UI.ImGui;
using KorpiEngine.Core.Windowing;
using OpenTK.Windowing.Common;

namespace KorpiEngine.Core;

/// <summary>
/// The main class for the application.
/// Contains the game window and handles the game loop.
/// </summary>
public static class Application
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Application));
    private static ImGuiController imGuiController = null!;
    private static double fixedFrameAccumulator;
    private static KorpiWindow window = null!;
    private static Scene initialScene = null!;


    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        LogFactory.Initialize(EngineConstants.LOG_4_NET_CONFIG_PATH);
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    public static void Run(WindowingSettings settings, Scene scene)
    {
        InitializeLog4Net();

        window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        imGuiController = new ImGuiController(window);
        initialScene = scene;
        
        window.Load += OnLoad;
        window.UpdateFrame += OnUpdateFrame;
        window.RenderFrame += OnRenderFrame;
        window.Unload += OnUnload;
        
        window.Run();
    }


    private static void OnLoad()
    {
        SceneManager.Initialize();
        GlobalJobPool.Initialize();
        
        // Queue window visibility after all internal resources are loaded.
        window.CenterWindow();
        window.IsVisible = true;
        
        SceneManager.LoadScene(initialScene, SceneLoadMode.Single);
    }


    private static void OnUpdateFrame(FrameEventArgs args)
    {
        KorpiProfiler.BeginFrame();
        KorpiProfiler.Begin("UpdateLoop");
        
        double deltaTime = args.Time;
        fixedFrameAccumulator += deltaTime;
        
        using (new ProfileScope("FixedUpdate"))
        {
            while (fixedFrameAccumulator >= EngineConstants.FIXED_DELTA_TIME)
            {
                InternalFixedUpdate();
                fixedFrameAccumulator -= EngineConstants.FIXED_DELTA_TIME;
            }
        }
 
        double fixedAlpha = fixedFrameAccumulator / EngineConstants.FIXED_DELTA_TIME;
        
        if (deltaTime > EngineConstants.MAX_DELTA_TIME)
        {
            Logger.Warn($"Detected large frame hitch ({1f/deltaTime:F2}fps, {deltaTime:F2}s)! Delta time was clamped to {EngineConstants.MAX_DELTA_TIME:F2} seconds.");
            deltaTime = EngineConstants.MAX_DELTA_TIME;
        }
        else if (deltaTime > EngineConstants.DELTA_TIME_SLOW_THRESHOLD)
        {
            Logger.Warn($"Detected frame hitch ({deltaTime:F2}s)!");
        }
        
        using (new ProfileScope("Update"))
        {
            InternalUpdate(deltaTime, fixedAlpha);
        }
        
        KorpiProfiler.End();
    }


    private static void OnRenderFrame(FrameEventArgs args)
    {
        KorpiProfiler.Begin("RenderLoop");
        
        InternalRender();
        
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
    }


    private static void InternalFixedUpdate()
    {
        Time.FixedUpdate();
        
        SceneManager.FixedUpdate();
        
        // Instantly execute jobs.
        GlobalJobPool.FixedUpdate();
    }


    private static void InternalUpdate(double deltaTime, double fixedAlpha)
    {
        Time.Update(deltaTime, fixedAlpha);
        Input.Update(window.KeyboardState, window.MouseState);
        
        ImGuiWindowManager.Update();
        imGuiController.Update();
        
        SceneManager.Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
    }


    private static void InternalRender()
    {
        SceneManager.Render();
        imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    private static void OnUnload()
    {
        SceneManager.UnloadAllScenes();
        GlobalJobPool.Shutdown();
        
        ImGuiWindowManager.Dispose();
        imGuiController.Dispose();
        window.Dispose();
    }
}
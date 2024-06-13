using System.Reflection;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.Debugging.Profiling;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering;
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
    private static ImGuiController imGuiController = null!;
    private static double fixedFrameAccumulator;
    private static Scene initialScene = null!;
    
    internal static KorpiWindow Window = null!;
    internal static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Application));
    
    public static string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    public static string AssetDirectory => Path.Combine(Directory, EngineConstants.ASSET_FOLDER_NAME);
    public static string DefaultsDirectory => Path.Combine(Directory, EngineConstants.DEFAULTS_FOLDER_NAME);
    public static string PackagesDirectory => Path.Combine(Directory, EngineConstants.PACKAGES_FOLDER_NAME);


    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        LogFactory.Initialize(Path.Combine(AssetDirectory, "log4net.config"));
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    public static void Run(WindowingSettings settings, Scene scene)
    {
        InitializeLog4Net();
        
        Window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        imGuiController = new ImGuiController(Window);
        initialScene = scene;
        
        Window.Load += OnLoad;
        Window.UpdateFrame += OnUpdateFrame;
        Window.RenderFrame += OnRenderFrame;
        Window.Unload += OnUnload;
        
        Window.Run();
    }
    
    
    public static void Quit()
    {
        Window.Close();
    }


    private static void OnLoad()
    {
        SceneManager.Initialize();
        GlobalJobPool.Initialize();
        
        // Queue window visibility after all internal resources are loaded.
        Window.CenterWindow();
        Window.IsVisible = true;
        
        AssemblyManager.Initialize();
        OnAssemblyLoadAttribute.Invoke();
        
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
        Graphics.StartFrame();
        
        KorpiProfiler.Begin("RenderLoop");
        
        InternalRender();
        
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
        
        Window.SwapBuffers();
        
        Graphics.EndFrame();
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
        Input.Update(Window.KeyboardState, Window.MouseState);
        
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
        OnAssemblyUnloadAttribute.Invoke();
        SceneManager.UnloadAllScenes();
        GlobalJobPool.Shutdown();
        
        ImGuiWindowManager.Dispose();
        imGuiController.Dispose();
        Window.Dispose();
    }
}
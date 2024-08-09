using System.Reflection;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.Debugging.Profiling;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Threading.Pooling;
using KorpiEngine.Core.UI.DearImGui;
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
    private static KorpiWindow window = null!;
    private static Scene initialScene = null!;
    
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
        
        window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        initialScene = scene;
        
        window.Load += OnLoad;
        window.UpdateFrame += OnUpdateFrame;
        window.RenderFrame += OnRenderFrame;
        window.Unload += OnUnload;
        
        AssemblyManager.Initialize();
        OnApplicationLoadAttribute.Invoke();
        
        window.Run();
    }
    
    
    public static void Quit()
    {
        window.Close();
    }


    private static void OnLoad()
    {
        SceneManager.Initialize();
        GlobalJobPool.Initialize();
        
        // Queue window visibility after all internal resources are loaded.
        window.CenterWindow();
        window.IsVisible = true;
        imGuiController = new ImGuiController(window);
        
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
        
        imGuiController.Update();
        ImGuiWindowManager.Update();
        
        SceneManager.Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
    }


    private static void InternalRender()
    {
        SceneManager.Render();
        imGuiController.Render();
    }


    private static void OnUnload()
    {
        OnApplicationUnloadAttribute.Invoke();
        SceneManager.UnloadAllScenes();
        GlobalJobPool.Shutdown();
        
        ImGuiWindowManager.Dispose();
        imGuiController.Dispose();
        window.Dispose();
    }
}
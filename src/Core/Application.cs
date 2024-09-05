using System.Reflection;
using KorpiEngine.InputManagement;
using KorpiEngine.Logging;
using KorpiEngine.SceneManagement;
using KorpiEngine.Threading.Pooling;
using KorpiEngine.UI;
using KorpiEngine.UI.DearImGui;
using KorpiEngine.Windowing;
using OpenTK.Windowing.Common;

namespace KorpiEngine;

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
    
    public static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Application));
    
    public static string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    public static string AssetsDirectory => Path.Combine(Directory, EngineConstants.ASSETS_FOLDER_NAME);
    public static string DefaultsDirectory => Path.Combine(AssetsDirectory, EngineConstants.DEFAULTS_FOLDER_NAME);
    public static string WebAssetsDirectory => Path.Combine(Directory, EngineConstants.WEB_ASSETS_FOLDER_NAME);


    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        LogFactory.Initialize(Path.Combine(AssetsDirectory, "log4net.config"));
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
        
        GUI.Initialize();
#if TOOLS
        EditorGUI.Initialize();
#endif
    }


    private static void OnUpdateFrame(FrameEventArgs args)
    {
        double deltaTime = args.Time;
        fixedFrameAccumulator += deltaTime;
        
        while (fixedFrameAccumulator >= EngineConstants.FIXED_DELTA_TIME)
        {
            InternalFixedUpdate();
            fixedFrameAccumulator -= EngineConstants.FIXED_DELTA_TIME;
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
        
        InternalUpdate(deltaTime, fixedAlpha);
    }


    private static void OnRenderFrame(FrameEventArgs args)
    {
        InternalRender();
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
#if TOOLS
        EditorGUI.Deinitialize();
#endif
        GUI.Deinitialize();
        
        OnApplicationUnloadAttribute.Invoke();
        SceneManager.Shutdown();
        GlobalJobPool.Shutdown();
        
        ImGuiWindowManager.Shutdown();
        imGuiController.Dispose();
        window.Dispose();
    }
}
using System.Reflection;
using KorpiEngine.AssetManagement;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;
using KorpiEngine.Threading;
using KorpiEngine.Tools.Logging;
using KorpiEngine.UI;
using KorpiEngine.UI.DearImGui;
using KorpiEngine.Utils;
using OpenTK.Windowing.Common;

namespace KorpiEngine;

/// <summary>
/// The main class for the application.
/// Contains the game window and handles the game loop.
/// </summary>
public static class Application
{
    private static ImGuiController imGuiController = null!;
    private static KorpiWindow window = null!;
    private static Type initialSceneType = null!;
    private static double fixedFrameAccumulator;
    
    [ThreadStatic]
    internal static bool IsMainThread;
    
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
    public static void Run<T>(WindowingSettings settings) where T : Scene
    {
        IsMainThread = true;
        
        InitializeLog4Net();
        AssemblyManager.Initialize();
        
        // Needs to be executed before Window.OnLoad() (called right after Window.Run())
        // to provide access to asset loading.
        AssetImporterAttribute.GenerateLookUp();
        
        window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        initialSceneType = typeof(T);
        
        window.Load += OnLoad;
        window.UpdateFrame += OnUpdateFrame;
        window.RenderFrame += OnRenderFrame;
        window.Unload += OnUnload;
        
        window.Run();
    }
    
    
    public static void Quit()
    {
        window.Close();
    }


    private static void OnLoad()
    {
        // Queue window visibility after all internal resources are loaded.
        window.CenterWindow();
        window.IsVisible = true;
        imGuiController = new ImGuiController(window);
        
        GUI.Initialize();
#if TOOLS
        EditorGUI.Initialize();
#endif
        GlobalJobPool.Initialize();
        SceneManager.Initialize(initialSceneType);
        OnApplicationLoadAttribute.Invoke();
    }


    private static void OnUnload()
    {
        OnApplicationUnloadAttribute.Invoke();
#if TOOLS
        EditorGUI.Deinitialize();
#endif
        GUI.Deinitialize();
        
        SceneManager.Shutdown();
        GlobalJobPool.Shutdown();
        AssetImporterAttribute.ClearLookUp();
        
        ImGuiWindowManager.Shutdown();
        imGuiController.Dispose();
        window.Dispose();
    }


#region Frame Updating and Rendering

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
        Input.Input.Update(window.KeyboardState, window.MouseState);
        
        SceneManager.Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
        
        imGuiController.Update();
        ImGuiWindowManager.Update();
    }


    private static void InternalRender()
    {
        SceneManager.Render();
        imGuiController.Render();
    }

#endregion
}
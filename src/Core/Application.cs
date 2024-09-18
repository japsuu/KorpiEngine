using System.Reflection;
using System.Text;
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
    private static AssetProvider? assetProviderInstance;
    private static SceneManager? sceneManagerInstance;
    private static KorpiWindow windowInstance = null!;
    private static Type initialSceneType = null!;
    private static double fixedFrameAccumulator;
    
    [ThreadStatic]
    internal static bool IsMainThread;
    public static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Application));
    
    public static string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    public static string AssetsDirectory => Path.Combine(Directory, EngineConstants.ASSETS_FOLDER_NAME);
    public static string DefaultsDirectory => Path.Combine(AssetsDirectory, EngineConstants.DEFAULTS_FOLDER_NAME);
    public static string WebAssetsDirectory => Path.Combine(Directory, EngineConstants.WEB_ASSETS_FOLDER_NAME);

    /// <summary>
    /// The currently active scene manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the scene manager has not been initialized yet.</exception>
    public static SceneManager SceneManager
    {
        get
        {
            if (sceneManagerInstance == null)
                throw new InvalidOperationException("The scene manager has not been initialized yet!");
            return sceneManagerInstance;
        }
    }
    
    
    /// <summary>
    /// The currently active asset provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the asset provider has not been initialized yet.</exception>
    public static AssetProvider AssetProvider
    {
        get
        {
            if (assetProviderInstance == null)
                throw new InvalidOperationException("The asset provider has not been initialized yet!");
            return assetProviderInstance;
        }
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    /// <typeparam name="T">The type of the initial scene to load.</typeparam>
    /// <param name="settings">The settings for the game window.</param>
    /// <param name="assetProvider">The asset provider to use for loading assets.</param>
    public static void Run<T>(WindowingSettings settings, AssetProvider assetProvider) where T : Scene
    {
        IsMainThread = true;
        MemoryReleaseSystem.Initialize();
        
        InitializeLog4Net();
        AssemblyManager.Initialize();
        
        // Needs to be executed before Window.OnLoad() (called right after Window.Run())
        // to provide access to asset loading.
        assetProviderInstance = assetProvider;
        assetProviderInstance.Initialize();
        
        initialSceneType = typeof(T);
        
        windowInstance = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        windowInstance.Load += OnLoad;
        windowInstance.UpdateFrame += OnUpdateFrame;
        windowInstance.RenderFrame += OnRenderFrame;
        windowInstance.Unload += OnUnload;
        
        windowInstance.Run();
    }
    
    
    /// <summary>
    /// Quits the application.
    /// </summary>
    public static void Quit()
    {
        windowInstance.Close();
    }


#region Loading and Unloading

    private static void OnLoad()
    {
        // Queue window visibility after all internal resources are loaded.
        windowInstance.CenterWindow();
        windowInstance.IsVisible = true;
        sceneManagerInstance = new SceneManager(initialSceneType);
        imGuiController = new ImGuiController(windowInstance);
        
        GUI.Initialize();
#if TOOLS
        EditorGUI.Initialize();
#endif
        GlobalJobPool.Initialize();
        OnApplicationLoadAttribute.Invoke();
    }


    private static void OnUnload()
    {
        OnApplicationUnloadAttribute.Invoke();
#if TOOLS
        EditorGUI.Deinitialize();
#endif
        GUI.Deinitialize();
        
        sceneManagerInstance?.InternalDispose();
        sceneManagerInstance = null;
        GlobalJobPool.Shutdown();
        assetProviderInstance?.Shutdown();
        
        ImGuiWindowManager.Shutdown();
        imGuiController.Dispose();
        MemoryReleaseSystem.Shutdown();
        windowInstance.Dispose();
    }

#endregion


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
        Input.Input.Update(windowInstance.KeyboardState, windowInstance.MouseState);
        
        SceneManager.Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
        
        imGuiController.Update();
        ImGuiWindowManager.Update();
        
        MemoryReleaseSystem.ProcessDisposeQueue();
    }


    private static void InternalRender()
    {
        SceneManager.Render();
        imGuiController.Render();
    }

#endregion


#region Utility

    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        LogFactory.Initialize(Path.Combine(AssetsDirectory, "log4net.config"));
    }

#endregion
}
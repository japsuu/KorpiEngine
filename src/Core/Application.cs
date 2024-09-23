using System.Reflection;
using System.Text;
using KorpiEngine.AssetManagement;
using KorpiEngine.InputManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;
using KorpiEngine.Threading;
using KorpiEngine.Tools.Logging;
using KorpiEngine.UI;
using KorpiEngine.UI.DearImGui;
using KorpiEngine.Utils;

namespace KorpiEngine;

/// <summary>
/// The main class for the application.
/// Contains the game window and handles the game loop.
/// </summary>
public static class Application
{
    private static GraphicsContext? graphicsContext;
    private static AssetProvider? assetProvider;
    private static SceneManager? sceneManager;
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
            if (sceneManager == null)
                throw new InvalidOperationException("The scene manager has not been initialized yet!");
            return sceneManager;
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
            if (assetProvider == null)
                throw new InvalidOperationException("The asset provider has not been initialized yet!");
            return assetProvider;
        }
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    /// <typeparam name="T">The type of the initial scene to load.</typeparam>
    /// <param name="settings">The settings for the game window.</param>
    /// <param name="provider">The asset provider to use for loading assets.</param>
    /// <param name="context">The graphics context to use for rendering.</param>
    public static void Run<T>(WindowingSettings settings, AssetProvider provider, GraphicsContext context) where T : Scene
    {
        IsMainThread = true;
        MemoryReleaseSystem.Initialize();
        
        InitializeLog4Net();
        AssemblyManager.Initialize();
        
        // Needs to be executed before Window.OnLoad() (called right after Window.Run())
        // to provide access to asset loading.
        assetProvider = provider;
        assetProvider.Initialize();
        
        initialSceneType = typeof(T);
        
        graphicsContext = context;
        graphicsContext.Run(settings, OnLoad, OnUpdate, OnRender, OnUnload);
        graphicsContext = null;
    }


    private static void OnWindowResize(Int2 newSize)
    {
        Graphics.UpdateViewport(newSize.X, newSize.Y);
        
        WindowInfo.Update(graphicsContext!);
    }


    /// <summary>
    /// Quits the application.
    /// </summary>
    public static void Quit()
    {
        if (graphicsContext == null)
            throw new InvalidOperationException("The graphics context has not been initialized yet!");
        
        graphicsContext.Shutdown();
    }


#region Loading and Unloading

    private static void OnLoad()
    {
        SystemInfo.Initialize();
        Graphics.Initialize(graphicsContext!);
        
        // Queue window visibility after all internal resources are loaded.
        graphicsContext!.Window.OnResize += OnWindowResize;
        graphicsContext.Window.SetCentered();
        graphicsContext.Window.IsVisible = true;
        sceneManager = new SceneManager(initialSceneType);
        
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
        
        sceneManager?.InternalDispose();
        sceneManager = null;
        GlobalJobPool.Shutdown();
        assetProvider?.Shutdown();
        
        ImGuiWindowManager.Shutdown();
        MemoryReleaseSystem.Shutdown();
        
        Graphics.Shutdown();
    }

#endregion


#region Frame Updating and Rendering

    private static void OnUpdate(double deltaTime)
    {
        if (deltaTime > EngineConstants.MAX_DELTA_TIME)
        {
            Logger.Warn($"Detected large frame hitch ({1f/deltaTime:F2}fps, {deltaTime:F2}s)! Delta time was clamped to {EngineConstants.MAX_DELTA_TIME:F2} seconds.");
            deltaTime = EngineConstants.MAX_DELTA_TIME;
        }
        else if (deltaTime > EngineConstants.DELTA_TIME_SLOW_THRESHOLD)
        {
            Logger.Warn($"Detected frame hitch ({deltaTime:F2}s)!");
        }
        
        InternalPreUpdate(deltaTime);
        
        fixedFrameAccumulator += deltaTime;
        
        while (fixedFrameAccumulator >= EngineConstants.FIXED_DELTA_TIME)
        {
            InternalFixedUpdate();
            fixedFrameAccumulator -= EngineConstants.FIXED_DELTA_TIME;
        }
 
        double fixedAlpha = fixedFrameAccumulator / EngineConstants.FIXED_DELTA_TIME;
        
        InternalUpdate(fixedAlpha);
    }


    private static void OnRender()
    {
        Graphics.StartFrame();
        InternalRender();
        Graphics.EndFrame();
    }


    private static void InternalFixedUpdate()
    {
        Time.FixedUpdate();
        
        SceneManager.FixedUpdate();
        
        // Instantly execute jobs.
        GlobalJobPool.FixedUpdate();
    }


    private static void InternalPreUpdate(double deltaTime)
    {
        Time.Update(deltaTime);
        Input.Update(graphicsContext!.InputState);
        DisplayInfo.Update(graphicsContext.DisplayState);
        Cursor.Update(graphicsContext.Window.CursorState);
    }


    private static void InternalUpdate(double fixedAlpha)
    {
        Time.UpdateFixedAlpha(fixedAlpha);
        
        SceneManager.Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
        
        graphicsContext!.ImGuiRenderer.Update();
        ImGuiWindowManager.Update();
        
        MemoryReleaseSystem.ProcessDisposeQueue();
    }


    private static void InternalRender()
    {
        SceneManager.Render();
        graphicsContext!.ImGuiRenderer.Render();
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


    internal static void SetCursorState(CursorLockState value)
    {
        graphicsContext!.Window.CursorState = value;
    }
}
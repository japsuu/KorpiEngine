using KorpiEngine.AssetManagement;
using KorpiEngine.Tools.Logging;

namespace KorpiEngine.SceneManagement;

/// <summary>
/// Manages in-game scenes.
/// More info: https://rivermanmedia.com/object-oriented-game-programming-the-scene-system/
/// </summary>
public static class SceneManager
{
    private readonly struct SceneLoadOperation(Scene scene, SceneLoadMode mode)
    {
        public readonly Scene Scene = scene;
        public readonly SceneLoadMode Mode = mode;
    }
    
    
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(SceneManager));
    private static readonly List<Scene> LoadedScenes = [];
    private static readonly Queue<SceneLoadOperation> OperationQueue = [];
    private static Scene? currentScene;

    /// <summary>
    /// The scene that was last loaded.
    /// </summary>
    public static Scene CurrentScene => currentScene ?? throw new InvalidOperationException("No scene is currently loaded!");

    /// <summary>
    /// All scenes that are currently loaded.
    /// </summary>
    public static IEnumerable<Scene> CurrentlyLoadedScenes => LoadedScenes;
    

    public static void Initialize()
    {
        LoadScene<EmptyScene>(SceneLoadMode.Single);
    }

    
    /// <summary>
    /// Loads the specified scene.
    /// The scene loading will be deferred until the next frame.
    /// </summary>
    /// <param name="mode">The mode in which to load the scene.</param>
    /// <exception cref="ArgumentNullException">Thrown if the scene is null.</exception>
    public static void LoadScene<T>(SceneLoadMode mode) where T : Scene, new()
    {
        Scene scene = new T();
        OperationQueue.Enqueue(new SceneLoadOperation(scene, mode));
    }
    
    
    public static void LoadScene(Type type, SceneLoadMode mode)
    {
        Scene? scene = (Scene?)Activator.CreateInstance(type);
        
        if (scene == null)
            throw new InvalidOperationException($"Failed to create an instance of the scene '{type.Name}'.");
        
        OperationQueue.Enqueue(new SceneLoadOperation(scene, mode));
    }


    private static void LoadSceneInternal(Scene scene, SceneLoadMode mode)
    {
        switch (mode)
        {
            case SceneLoadMode.Single:
            {
                LoadSceneSingle(scene);
                break;
            }
            case SceneLoadMode.Additive:
            {
                LoadSceneAdditive(scene);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        Logger.Debug($"Loaded scene '{scene.GetType().Name}' as {mode}.");
    }


    private static void LoadSceneSingle(Scene scene)
    {
        // Unload the old scenes.
        foreach (Scene loadedScene in LoadedScenes)
            loadedScene.Destroy();
        
        LoadedScenes.Clear();

        // Set the new scene as the current scene.
        currentScene = scene;
        LoadedScenes.Add(CurrentScene);

        // Load the new scene.
        scene.InternalLoad();
    }


    private static void LoadSceneAdditive(Scene scene)
    {
        // Set the new scene as the current scene.
        currentScene = scene;
        LoadedScenes.Add(CurrentScene);

        // Load the new scene.
        scene.InternalLoad();
    }
    
    
    internal static void Update()
    {
        Asset.ProcessDisposeQueue();
        
        while (OperationQueue.Count > 0)
        {
            SceneLoadOperation operation = OperationQueue.Dequeue();
            LoadSceneInternal(operation.Scene, operation.Mode);
        }
        
        CurrentScene.InternalUpdate();
    }
    
    
    internal static void FixedUpdate()
    {
        CurrentScene.InternalFixedUpdate();
    }
    
    
    internal static void Render()
    {
        CurrentScene.InternalRender();
    }
    
    
    internal static void Shutdown()
    {
#warning TODO: Implement DontDestroyOnLoad
        
        // Unload all scenes and destroy all objects in them.
        foreach (Scene loadedScene in LoadedScenes)
            loadedScene.Destroy();
        
        // Handle all objects that were just destroyed.
        Asset.ProcessDisposeQueue();
        
        LoadedScenes.Clear();
        currentScene = null;
    }
}
using KorpiEngine.AssetManagement;

namespace KorpiEngine.SceneManagement;

/// <summary>
/// Manages in-game scenes.
/// </summary>
// More info: https://rivermanmedia.com/object-oriented-game-programming-the-scene-system/
public class SceneManager
{
    private readonly struct SceneLoadOperation(Scene scene, SceneLoadMode mode)
    {
        public readonly Scene Scene = scene;
        public readonly SceneLoadMode Mode = mode;
    }
    
    
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
    

    /// <summary>
    /// Initializes the scene manager with the given scene.
    /// </summary>
    internal static void Initialize(Type type)
    {
        LoadScene(type, SceneLoadMode.Single);
    }

    
    /// <summary>
    /// Loads a new scene of the specified type.
    /// The scene loading will be deferred until the next frame.
    /// </summary>
    /// <typeparam name="T">The type of the scene to load.</typeparam>
    /// <param name="mode">The mode in which to load the scene.</param>
    public static void LoadScene<T>(SceneLoadMode mode) where T : Scene, new()
    {
        Scene scene = new T();
        OperationQueue.Enqueue(new SceneLoadOperation(scene, mode));
    }
    
    
    /// <summary>
    /// Loads a new scene of the specified type.
    /// The scene loading will be deferred until the next frame.
    /// </summary>
    /// <param name="type">The type of the scene to load.</param>
    /// <param name="mode">The mode in which to load the scene.</param>
    /// <exception cref="InvalidOperationException">Thrown if the scene could not be created.</exception>
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

        Application.Logger.Debug($"Loaded scene '{scene.GetType().Name}' as {mode}.");
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
        EngineObject.ProcessDisposeQueue();
        
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
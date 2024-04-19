using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// Manages in-game scenes.
/// More info: https://rivermanmedia.com/object-oriented-game-programming-the-scene-system/
/// </summary>
public static class SceneManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(SceneManager));
    private static readonly List<Scene> LoadedScenes = new();
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
        LoadScene(new EmptyScene(), SceneLoadMode.Single);
    }

    
    public static void LoadScene(Scene scene, SceneLoadMode mode)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        
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
            loadedScene.Dispose();

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
        CurrentScene.InternalUpdate();
    }
    
    
    internal static void FixedUpdate()
    {
        CurrentScene.InternalFixedUpdate();
    }
    
    
    internal static void Draw()
    {
        CurrentScene.InternalDraw();
    }
    
    
    internal static void UnloadAllScenes()
    {
        foreach (Scene loadedScene in LoadedScenes)
            loadedScene.Dispose();
        
        LoadedScenes.Clear();
        currentScene = null;
    }
}
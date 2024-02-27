using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// Manages in-game scenes.
/// More info: https://rivermanmedia.com/object-oriented-game-programming-the-scene-system/
/// </summary>
public static class SceneManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(SceneManager));
    
    public static Scene? CurrentScene { get; private set; }
    
    
    public static void LoadScene(Scene scene)
    {
        CurrentScene?.Unload();

        scene.Load();
        CurrentScene = scene;
        Logger.Info($"Loaded scene: {scene.GetType().Name}");
    }
    
    
    internal static void Update()
    {
        CurrentScene?.Update();
    }
    
    
    internal static void FixedUpdate()
    {
        CurrentScene?.FixedUpdate();
    }
    
    
    internal static void Draw()
    {
        CurrentScene?.Draw();
    }
}
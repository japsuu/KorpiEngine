namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// Represents an in-game scene.
/// </summary>
public abstract class Scene
{
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    public abstract void Load();
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    public abstract void Unload();
    
    /// <summary>
    /// Called from the update loop,
    /// </summary>
    public abstract void Update();
    
    /// <summary>
    /// Called from the fixed update loop.
    /// </summary>
    public abstract void FixedUpdate();
    
    /// <summary>
    /// Called when the scene should be drawn.
    /// </summary>
    public abstract void Draw();
}
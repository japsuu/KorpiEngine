namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// Determines how to load a scene.
/// The new scene will always be loaded before any old scenes are unloaded.
/// </summary>
public enum SceneLoadMode
{
    /// <summary>
    /// This scene will replace all currently loaded scenes, unloading them in the process.
    /// </summary>
    Single,
    
    /// <summary>
    /// This scene will be loaded on top of all other pre-existing scenes.
    /// </summary>
    Additive
}
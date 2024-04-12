namespace KorpiEngine.Core;

/// <summary>
/// Contains constants used throughout the engine.
/// </summary>
public static class EngineConstants
{
    public const string ENGINE_NAME = "Korpi Engine";
    public const string ENGINE_VERSION = "Dev";

    #region UPDATE LOOP

    /// <summary>
    /// The maximum number of fixed updates to be executed per second.
    /// </summary>
    public const int FIXED_UPDATE_FRAME_FREQUENCY = 20;
    
    /// <summary>
    /// The amount of time (in seconds) between each fixed update.
    /// </summary>
    public const float FIXED_DELTA_TIME = 1f / FIXED_UPDATE_FRAME_FREQUENCY;

    /// <summary>
    /// The threshold at which the engine will warn the user that the update loop is running too slowly.
    /// Default: 10fps
    /// </summary>
    public const float DELTA_TIME_SLOW_THRESHOLD = 0.1f;

    /// <summary>
    /// An upper limit on the amount of time the engine will report as having passed by the <see cref="Time.DeltaTimeDouble"/>.
    /// </summary>
    public const float MAX_DELTA_TIME = 0.5f;

    #endregion

    #region RENDERING

    /// <summary>
    /// The maximum amount of textures that can be loaded simultaneously.
    /// </summary>
    public const int MAX_SUPPORTED_TEXTURES = 1024;
    
    /// <summary>
    /// The level of anisotropic filtering to use for textures.
    /// </summary>
    public const int ANISOTROPIC_FILTERING_LEVEL = 16;
    
    /// <summary>
    /// The base path of the internal shader files, relative to the project root.
    /// </summary>
    public const string INTERNAL_SHADER_BASE_PATH = "assets/shaders/";

    #endregion
}
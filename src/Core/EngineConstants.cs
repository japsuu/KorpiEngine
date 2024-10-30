using KorpiEngine.Utils;

namespace KorpiEngine;

/// <summary>
/// Contains constants used throughout the engine.
/// </summary>
public static class EngineConstants
{
    public const string ENGINE_NAME = "Korpi Engine";
    public const string ENGINE_VERSION = "Dev";
    public const string DEFAULT_SHADER_DEFINE = $"KORPI_{ENGINE_VERSION}";
    public const string ASSETS_FOLDER_NAME = "Assets";
    public const string DEFAULTS_FOLDER_NAME = "Defaults";
    public const string WEB_ASSETS_FOLDER_NAME = "WebAssets";

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
}
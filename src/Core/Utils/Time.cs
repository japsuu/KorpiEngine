﻿namespace KorpiEngine.Utils;

public static class Time
{
    /// <summary>
    /// Time in seconds that has passed since the last frame, as a float.
    /// </summary>
    public static float DeltaTime { get; private set; }
    
    /// <summary>
    /// Time in seconds that has passed since the last frame, as a double.
    /// </summary>
    public static double DeltaTimeDouble { get; private set; }
    
    /// <summary>
    /// Time in seconds that has passed since the last fixed frame.
    /// </summary>
    public static double FixedDeltaTime => EngineConstants.FIXED_DELTA_TIME;

    /// <summary>
    /// Time in seconds that has passed since the last fixed frame, as a float.
    /// </summary>
    public static float FixedDeltaTimeFloat => EngineConstants.FIXED_DELTA_TIME;
    
    /// <summary>
    /// Total time in seconds that has passed since the start of the game.
    /// </summary>
    public static double TotalTime { get; private set; }
    
    /// <summary>
    /// Total number of frames that have passed since the start of the game.
    /// </summary>
    public static int TotalFrameCount { get; private set; }
    
    /// <summary>
    /// Total number of fixed frames that have passed since the start of the game.
    /// </summary>
    public static int TotalFixedFrameCount { get; private set; }

    /// <summary>
    /// This value stores how far we are in the current update frame, relative to the fixed update loop.
    /// For example, when the value of <see cref="FixedAlpha"/> is 0.5, it means we are halfway between the last frame and the next upcoming frame.
    /// </summary>
    public static double FixedAlpha { get; private set; }
    
    public static float FrameRate { get; private set; }


    internal static void Update(double deltaTime)
    {
        DeltaTimeDouble = deltaTime;
        DeltaTime = (float) deltaTime;
        FrameRate = 1.0f / DeltaTime;
        
        TotalTime += deltaTime;
        
        TotalFrameCount++;
    }


    internal static void UpdateFixedAlpha(double fixedAlpha)
    {
        FixedAlpha = fixedAlpha;
    }


    internal static void FixedUpdate()
    {
        TotalFixedFrameCount++;
    }


    public static void Reset()
    {
        DeltaTimeDouble = 0;
        DeltaTime = 0;
        TotalTime = 0;
        TotalFrameCount = 0;
        TotalFixedFrameCount = 0;
        FixedAlpha = 0;
        FrameRate = 0;
    }
}
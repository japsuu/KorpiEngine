using System.Diagnostics;

namespace KorpiEngine.Core.Debugging.Profiling;

/// <summary>
/// A simple profiler that can be used to measure the duration of code execution.
/// This is not optimized, and will allocate huge amounts of memory if used incorrectly.
/// NOTE: This is not thread-safe.
/// </summary>
public static class KorpiProfiler
{
    private const bool ENABLE_PROFILING = true;
    public static bool IsProfilingEnabled = false;
    
    private static readonly Stack<Profile> Profiles = new();
    private static Profile? lastFrame;
    private static bool internalEnabled;
    
    
    public static Profile? GetLastFrame() => lastFrame;


    public static void BeginFrame()
    {
        if (!ENABLE_PROFILING)
            return;

        if (internalEnabled != IsProfilingEnabled)
        {
            internalEnabled = IsProfilingEnabled;
            if (!internalEnabled)
                Profiles.Clear();
        }
        Begin("Frame");
    }


    public static void Begin(string name)
    {
        if (!internalEnabled || !ENABLE_PROFILING)
            return;

        if (Profiles.Count == 0 && name != "Frame")
            throw new InvalidOperationException("Cannot call Begin before BeginFrame.");
        
        Profiles.Push(new Profile(name, Stopwatch.StartNew(), 0));
    }


    public static void BeginConditional(string name, float discardIfDurationLessThanMillis)
    {
        if (!internalEnabled || !ENABLE_PROFILING)
            return;

        if (Profiles.Count == 0 && name != "Frame")
            throw new InvalidOperationException("Cannot call Begin before BeginFrame.");
        
        Profiles.Push(new Profile(name, Stopwatch.StartNew(), discardIfDurationLessThanMillis));
    }


    public static void End()
    {
        if (!internalEnabled || !ENABLE_PROFILING)
            return;

        if (Profiles.Count == 1)
            throw new InvalidOperationException("Cannot call End without a matching Begin.");

        Profile profile = Profiles.Pop();
        profile.Stopwatch.Stop();
        profile.DurationMillis = profile.Stopwatch.Elapsed.TotalMilliseconds;
        
        if (profile.DiscardIfDurationLessThan > 0 && profile.DurationMillis < profile.DiscardIfDurationLessThan)
            return;

        Profiles.Peek().Children.Add(profile);
    }
    
    
    public static void EndFrame()
    {
        if (!internalEnabled || !ENABLE_PROFILING)
            return;

        if (Profiles.Count > 1)
            throw new InvalidOperationException("Cannot end frame while there are active profiles.");

        Profile profile = Profiles.Pop();
        profile.Stopwatch.Stop();
        profile.DurationMillis = profile.Stopwatch.Elapsed.TotalMilliseconds;
        lastFrame = profile;
    }
}
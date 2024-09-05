namespace KorpiEngine.Threading.Threads;

/// <summary>
/// Parameters for thread states.
/// A value of -1 for a specific state will disable that state and any others below it.
/// </summary>
public struct ThreadConfig(int spinCycles, int yieldCycles, int napCycles, int napInterval, int sleepInterval)
{
    /// <summary>
    /// Number of spin cycles before the thread drops to the yielding state.
    /// </summary>
    public int SpinCycles { get; set; } = spinCycles;

    /// <summary>
    /// Number of yield cycles before the thread drops to the napping state.
    /// </summary>
    public int YieldCycles { get; set; } = yieldCycles;

    /// <summary>
    /// Number of nap cycles before the thread drops to the sleeping state.
    /// </summary>
    public int NapCycles { get; set; } = napCycles;

    /// <summary>
    /// How long the thread should sleep in milliseconds between nap cycles.
    /// </summary>
    public int NapInterval { get; set; } = napInterval;

    /// <summary>
    /// How long the thread should sleep in milliseconds between sleep cycles.
    /// </summary>
    public readonly int SleepInterval = sleepInterval;


    public static ThreadConfig Default()
    {
        return new ThreadConfig(
            100,
            500,
            5,
            1,
            5
        );
    }
}
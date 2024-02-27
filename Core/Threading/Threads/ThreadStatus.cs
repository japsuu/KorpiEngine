namespace KorpiEngine.Core.Threading.Threads;

/// <summary>
/// Status of a <see cref="WorkerThread"/>.
/// </summary>
public enum ThreadStatus
{
    /// <summary>
    /// Thread is actively spinning and checking for work.
    /// </summary>
    Spinning = 0,

    /// <summary>
    /// Thread is checking for work but yielding if there is none.
    /// </summary>
    Yielding = 1,

    /// <summary>
    /// Thread is sleeping for short intervals while periodically checking for work.
    /// </summary>
    Napping = 2,

    /// <summary>
    /// Thread is sleeping for longer intervals while periodically checking for work.
    /// </summary>
    Sleeping = 3,

    /// <summary>
    /// Thread has completed shutting down.
    /// </summary>
    Offline = 4
}
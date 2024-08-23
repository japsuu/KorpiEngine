namespace KorpiEngine.Core.Threading.Pooling;

/// <summary>
/// Queue type used by the thread-pool to handle updates pushed to main.
/// </summary>
public enum QueueType
{
    /// <summary>
    /// Executed in a batch with all other actions each frame.
    /// </summary>
    Default,

    /// <summary>
    /// Only executed if within the invocation budget of the current frame.
    /// Useful for expensive operations such as instantiation to avoid slamming Unity with a ton of work from
    /// a large number of jobs.
    /// </summary>
    Throttled
}
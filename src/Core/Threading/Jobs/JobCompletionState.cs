namespace KorpiEngine.Core.Threading.Jobs;

/// <summary>
/// Represents the completion state of a <see cref="IKorpiJob"/>.
/// </summary>
public enum JobCompletionState
{
    /// <summary>
    /// The job has not been completed yet.
    /// </summary>
    None,
    
    /// <summary>
    /// The job has been completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// The job has been aborted.
    /// </summary>
    Aborted
}
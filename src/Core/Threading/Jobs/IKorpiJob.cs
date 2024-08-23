namespace KorpiEngine.Core.Threading.Jobs;

/// <summary>
/// Represents a job that can be executed by a <see cref="ThreadPool"/>.
/// </summary>
public interface IKorpiJob
{
    /// <summary>
    /// The current completion state of the job.
    /// </summary>
    public JobCompletionState CompletionState { get; }
    
    /// <summary>
    /// Returns the current priority of the job.
    /// </summary>
    /// <returns></returns>
    public float GetPriority();

    /// <summary>
    /// Executes the job.
    /// </summary>
    public void Execute();

    /// <summary>
    /// Signals the job that it has been completed.
    /// </summary>
    /// <param name="completionState">The new completion state of the job.</param>
    public void SignalCompletion(JobCompletionState completionState);
}
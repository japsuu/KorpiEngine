using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Pooling;

namespace KorpiEngine.Core.Threading.Jobs;

/// <summary>
/// A job for the <see cref="JobThreadPool"/> with a generic result type.
/// </summary>
public abstract class KorpiJob<T> : IKorpiJob, IAwaitable<T>
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(KorpiJob));

    private Action? _continuation;
    private T _result;

    /// <summary>
    /// Completion state of the job.
    /// </summary>
    public JobCompletionState CompletionState { get; protected set; }


    /// <summary>
    /// Create a new KorpiJob instance.
    /// The context of the constructing thread is stored for async/await.
    /// Make sure you construct your job on the same thread that will await the result.
    /// </summary>
    protected KorpiJob()
    {
        _continuation = null;
        _result = default!;
        CompletionState = JobCompletionState.None;
    }


    public abstract float GetPriority();


    /// <summary>
    /// Do your main work here.
    /// Anything used within this method should be thread-safe.
    /// </summary>
    public abstract void Execute();


    /// <summary>
    /// Sets the result of the job.
    /// </summary>
    protected void SetResult(T result)
    {
        _result = result;
    }


    /// <summary>
    /// Signals that the job has completed.
    /// The thread-pool will call this automatically with a state of "Completed" unless you call it explicitly.
    /// </summary>
    public void SignalCompletion(JobCompletionState completionState)
    {
        if (completionState == JobCompletionState.None)
        {
            Logger.Error("Signal completion called with a state of 'None'!");
            return;
        }

        if (CompletionState != JobCompletionState.None)
        {
            Logger.Error("Signal completion called multiple times in job!");
            return;
        }

        CompletionState = completionState;
        Action? continuation = Interlocked.Exchange(ref _continuation, null);
        if (continuation != null)
            DispatchToMain(continuation, QueueType.Default);
    }


    /// <summary>
    /// Dispatches the job to be executed by the thread-pool.
    /// </summary>
    public KorpiJob<T> Dispatch()
    {
        return GlobalJobPool.DispatchJob(this);
    }


    /// <summary>
    /// Dispatches a given action to be executed on the main thread.
    /// </summary>
    protected void DispatchToMain(Action a, QueueType queueType)
    {
        GlobalJobPool.DispatchOnMain(a, queueType);
    }


    // Custom awaiter pattern.
    public bool IsCompleted => CompletionState != JobCompletionState.None;


    public virtual T GetResult()
    {
        return _result;
    }


    public void OnCompleted(Action continuation)
    {
        Volatile.Write(ref _continuation, continuation);
    }


    public IAwaitable<T> GetAwaiter()
    {
        return this;
    }


    /// <summary>
    /// Blocks the caller until all specified jobs have completed.
    /// </summary>
    public static void WhenAll(IEnumerable<IKorpiJob> jobs)
    {
        IEnumerable<IKorpiJob> korpiJobs = jobs.ToList();
        bool isComplete = false;
        while (!isComplete)
        {
            isComplete = true;
            foreach (IKorpiJob job in korpiJobs)
            {
                if (job.CompletionState != JobCompletionState.None) continue;
                isComplete = false;
                break;
            }
        }
    }
}

/// <summary>
/// Basic implementation of a KorpiJob who's result is just the completion state.
/// </summary>
public abstract class KorpiJob : KorpiJob<JobCompletionState>
{
    public override JobCompletionState GetResult()
    {
        return CompletionState;
    }
}
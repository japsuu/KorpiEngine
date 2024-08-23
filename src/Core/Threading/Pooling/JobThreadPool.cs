using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Jobs;
using KorpiEngine.Core.Threading.Threads;

namespace KorpiEngine.Core.Threading.Pooling;

/// <summary>
/// Custom thread pool with progressive throttling.
/// </summary>
public sealed class JobThreadPool : IJobPool
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(JobThreadPool));

    private readonly List<WorkerThread> _workers;
    private readonly PriorityWorkQueue<IKorpiJob> _workQueue;


    /// <summary>
    /// Creates a new thread pool with the desired number of worker threads.
    /// </summary>
    /// <param name="threadCount">Number of workers threads to allocate.</param>
    /// <param name="config">The thread config to use for the workers threads.</param>
    public JobThreadPool(uint threadCount, ThreadConfig config)
    {
        if (threadCount == 0)
            throw new ArgumentException("Thread count must be greater than zero!");

        _workQueue = new PriorityWorkQueue<IKorpiJob>();
        _workers = new List<WorkerThread>();
        
        for (int i = 0; i < threadCount; i++)
            _workers.Add(new WorkerThread(_workQueue, config));
    }


    public void EnqueueWorkItem(IKorpiJob korpiJob)
    {
        if (_workQueue.IsAddingCompleted)
            throw new InvalidOperationException("Cannot queue a work item if the pool is shutting down.");

        _workQueue.Add(korpiJob, korpiJob.GetPriority());
    }


    /// <summary>
    /// Disables adding of work items and waits for all workers to shutdown.
    /// You should only call this once you are done adding any work items.
    /// </summary>
    public void Shutdown()
    {
        _workQueue.CompleteAdding();

        // Wait for workers to terminate.
        while (true)
        {
            foreach (WorkerThread worker in _workers)
            {
                if (worker.Status != ThreadStatus.Offline)
                    continue;
                return;
            }
        }
    }


    public void FixedUpdate()
    {
        
    }


    /// <summary>
    /// Disables adding of work items and informs all workers to shutdown immediately.
    /// If any jobs are queued, they will be ignored.
    /// If the desired wait interval is exceeded, the worker threads will be aborted.
    /// </summary>
    /// <param name="waitInterval">How many cycles to wait before aborting the threads.</param>
    public void ShutdownNow(uint waitInterval = 2000)
    {
        _workQueue.CompleteAdding();

        foreach (WorkerThread worker in _workers)
            worker.Shutdown();

        // Wait some period for workers to terminate.
        uint count = 0u;
        while (true)
        {
            if (count >= waitInterval)
            {
                Logger.Warn("Thread pool shutdown wait interval exceeded, aborting threads.");
                break;
            }

            foreach (WorkerThread worker in _workers)
            {
                if (worker.Status != ThreadStatus.Offline) continue;
                return;
            }

            count++;
        }

        foreach (WorkerThread worker in _workers)
        {
            if (worker.Status == ThreadStatus.Offline) continue;
            worker.Abort();
        }
    }
}
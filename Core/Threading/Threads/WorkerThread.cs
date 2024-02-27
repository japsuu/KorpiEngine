using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Jobs;
using KorpiEngine.Core.Threading.Pooling;

namespace KorpiEngine.Core.Threading.Threads;

public sealed class WorkerThread
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(WorkerThread));

    private readonly PriorityWorkQueue<IKorpiJob> _workQueue;
    private readonly ThreadConfig _config;
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts;

    private uint _cycleCounter;
    private bool _shuttingDown;

    /// <summary>
    /// Configuration struct used by this thread.
    /// </summary>
    public ThreadConfig Config => _config;

    /// <summary>
    /// Current status of the thread.
    /// If the status is 'Error' more info can be obtained from <see cref="LastException"/>.
    /// </summary>
    public ThreadStatus Status { get; private set; }

    /// <summary>
    /// The last exception the thread encountered.
    /// </summary>
    public Exception? LastException { get; private set; }


    public WorkerThread(PriorityWorkQueue<IKorpiJob> workQueue, ThreadConfig config)
    {
        _workQueue = workQueue;
        _config = config;
        _cts = new CancellationTokenSource();

        _thread = new Thread(WorkLoop)
        {
            IsBackground = true
        };
        _thread.Name = $"Pool Worker Thread #{_thread.ManagedThreadId}";
        _thread.Start(_cts.Token);

        _cycleCounter = 0;
        Status = ThreadStatus.Spinning;
    }


    /// <summary>
    /// Tells the worker to shutdown at the next cycle.
    /// </summary>
    public void Shutdown()
    {
        _shuttingDown = true;
    }


    /// <summary>
    /// Immediately aborts the workers thread.
    /// </summary>
    public void Abort()
    {
        Status = ThreadStatus.Offline;
        _cts.Cancel();
        _thread.Interrupt();
        _thread.Join();
    }


    private void WorkLoop(object? obj)
    {
        if (obj is null)
        {
            Logger.Warn("Worker thread has been started without a cancellation token.");
            return;
        }
        
        CancellationToken ct = (CancellationToken)obj;
        
        while (!ct.IsCancellationRequested)
        {
            // Shut down if there is no more work being added.
            if (_shuttingDown || _workQueue.IsCompleted)
            {
                Status = ThreadStatus.Offline;
                return;
            }

            // Check for work.
            bool hasWork = _workQueue.TryTakeFromAny(out IKorpiJob? job);

            if (hasWork && job == null)
            {
                Logger.Error("Worker thread has encountered a null job.");
                continue;
            }

            // Try to invoke the job and reset the cycle counter and thread status.
            if (hasWork)
            {
                try
                {
                    job!.Execute();

                    if (job.CompletionState == JobCompletionState.None)
                        job.SignalCompletion(JobCompletionState.Completed);
                }
                catch (Exception e)
                {
                    Logger.Error("Worker thread encountered an exception while executing a job:", e);
                    LastException = e;

                    if (job!.CompletionState == JobCompletionState.None)
                        job.SignalCompletion(JobCompletionState.Aborted);
                }

                _cycleCounter = 0;
                Status = ThreadStatus.Spinning;
                continue;
            }

            // Increment cycle counter if no work found.
            _cycleCounter++;

            // Branch based on current status.
            switch (Status)
            {
                case ThreadStatus.Spinning:
                    if (_config.SpinCycles != -1 && _cycleCounter >= _config.SpinCycles)
                    {
                        _cycleCounter = 0;
                        Status = ThreadStatus.Yielding;
                    }

                    break;
                case ThreadStatus.Yielding:
                    if (_config.YieldCycles != -1 && _cycleCounter >= _config.YieldCycles)
                    {
                        _cycleCounter = 0;
                        Status = ThreadStatus.Napping;
                    }

                    Thread.Yield();
                    break;
                case ThreadStatus.Napping:
                    if (_config.NapCycles != -1 && _cycleCounter >= _config.NapCycles)
                    {
                        _cycleCounter = 0;
                        Status = ThreadStatus.Sleeping;
                    }

                    Thread.Sleep(_config.NapInterval);
                    break;
                case ThreadStatus.Sleeping:
                    Thread.Sleep(_config.SleepInterval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
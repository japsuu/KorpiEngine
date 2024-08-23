using System.Collections.Concurrent;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Jobs;

namespace KorpiEngine.Core.Threading.Pooling;

public sealed class JobSingleThreadPool : IJobPool
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(JobSingleThreadPool));
    
    private readonly ConcurrentQueue<IKorpiJob> _workQueue = new();
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts;


    public JobSingleThreadPool()
    {
        _cts = new CancellationTokenSource();
        
        _thread = new Thread(WorkLoop)
        {
            IsBackground = true
        };
        _thread.Name = $"Pool Worker Thread #{_thread.ManagedThreadId}";
        _thread.Start(_cts.Token);
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
            if (_workQueue.TryDequeue(out IKorpiJob? job))
            {
                try
                {
                    job.Execute();

                    if (job.CompletionState == JobCompletionState.None)
                        job.SignalCompletion(JobCompletionState.Completed);
                }
                catch (Exception e)
                {
                    Logger.Error("Worker thread encountered an exception while executing a job:", e);

                    if (job.CompletionState == JobCompletionState.None)
                        job.SignalCompletion(JobCompletionState.Aborted);
                }
            }
        }
    }


    public void EnqueueWorkItem(IKorpiJob korpiJob)
    {
        _workQueue.Enqueue(korpiJob);
    }


    public void FixedUpdate()
    {
        
    }


    public void Shutdown()
    {
        _cts.Cancel();
        _thread.Join();
    }
}
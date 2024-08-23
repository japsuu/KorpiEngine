using System.Threading.Tasks.Dataflow;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Jobs;

namespace KorpiEngine.Core.Threading.Pooling;

public sealed class JobTplPool : IJobPool
{
    private const int MAX_JOBS_POSTED_PER_FRAME = 128;
    
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(JobSingleThreadPool));
    
    private readonly ActionBlock<IKorpiJob> _jobProcessor = new(
        ExecuteJob,
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount * 3 / 4,    // Allocate three quarters of the available threads to the job processor.
            BoundedCapacity = -1
        });
    // Dynamically switch between two queues, to allow dynamic priority changes.
    private readonly PriorityQueue<IKorpiJob, float> _workQueue1 = new();
    private readonly PriorityQueue<IKorpiJob, float> _workQueue2 = new();
    private PriorityQueue<IKorpiJob, float> _activeQueue;
    private bool _useSecondQueue;


    public JobTplPool()
    {
        _activeQueue = _workQueue1;
    }


    public void EnqueueWorkItem(IKorpiJob korpiJob)
    {
        _activeQueue.Enqueue(korpiJob, korpiJob.GetPriority());
    }


    public void FixedUpdate()
    {
        // Switch queues and update priorities.
        PriorityQueue<IKorpiJob, float> destination = _useSecondQueue ? _workQueue1 : _workQueue2;
        while (_activeQueue.TryDequeue(out IKorpiJob? job, out float _))
            destination.Enqueue(job, job.GetPriority());
        _useSecondQueue = !_useSecondQueue;
        _activeQueue = destination;
        
        // Process the job queue.
        int i = 0;
        while (i < MAX_JOBS_POSTED_PER_FRAME && _activeQueue.TryDequeue(out IKorpiJob? job, out float _))
        {
            if (!_jobProcessor.Post(job))
                Logger.Warn($"Failed to post a job ({job.GetType().FullName}) to the job processor.");
            i++;
        }
    }


    public void Shutdown()
    {
        _jobProcessor.Complete();
    }


    private static void ExecuteJob(IKorpiJob job)
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
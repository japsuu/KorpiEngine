using System.Collections.Concurrent;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Threading.Jobs;

namespace KorpiEngine.Core.Threading.Pooling;

/// <summary>
/// A static pool for executing jobs.
/// Contains queues for pushing callbacks to the main thread.
/// Jobs don't necessarily have to use the queues for main-thread callbacks.
/// A sync context could be used instead.
/// </summary>
public static class GlobalJobPool
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GlobalJobPool));

    /// <summary>
    /// Maximum number of <see cref="mainQueueThrottled"/> invocations executed per tick.
    /// </summary>
    private const int MAX_THROTTLED_UPDATES_PER_TICK = 64;

    /// <summary>
    /// The job pool.
    /// </summary>
    private static IJobPool jobPool = null!;

    /// <summary>
    /// A queue of actions to be executed on the main thread.
    /// </summary>
    private static ConcurrentQueue<Action> mainQueue = null!;

    /// <summary>
    /// A throttled queue of actions to be executed on the main thread.
    /// </summary>
    private static ConcurrentQueue<Action> mainQueueThrottled = null!;
    
    public static ulong ItemsInMainThreadQueue { get; private set; }
    public static ulong ItemsInMainThreadThrottledQueue { get; private set; }
    public static ulong MainThreadThrottledQueueItemsPerTick => MAX_THROTTLED_UPDATES_PER_TICK;


    internal static void Initialize()
    {
        jobPool = new JobTplPool();
        mainQueue = new ConcurrentQueue<Action>();
        mainQueueThrottled = new ConcurrentQueue<Action>();
    }
    
    
    internal static void Shutdown()
    {
        jobPool.Shutdown();
        Logger.Info("Shutdown.");
    }


    internal static void Update()
    {
        ItemsInMainThreadQueue = (ulong)mainQueue.Count;
        
        // Process the regular queue.
        while (mainQueue.TryDequeue(out Action? a))
            a.Invoke();
    }


    internal static void FixedUpdate()
    {
        ItemsInMainThreadThrottledQueue = (ulong)mainQueueThrottled.Count;

        // Process the throttled queue.
        int count = MAX_THROTTLED_UPDATES_PER_TICK;
        while (mainQueueThrottled.TryDequeue(out Action? a))
        {
            a.Invoke();
            count--;

            if (count <= 0)
                break;
        }
        
        jobPool.FixedUpdate();
    }


    /// <summary>
    /// Immediately queues the provided job for execution on the pool.
    /// </summary>
    public static KorpiJob<T> DispatchJob<T>(KorpiJob<T> item)
    {
        jobPool.EnqueueWorkItem(item);
        return item;
    }


    /// <summary>
    /// Queues a given action to be executed on the main thread.
    /// </summary>
    public static void DispatchOnMain(Action a, QueueType queue)
    {
        switch (queue)
        {
            case QueueType.Default:
                mainQueue.Enqueue(a);
                break;
            case QueueType.Throttled:
                mainQueueThrottled.Enqueue(a);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(queue), queue, null);
        }
    }
}
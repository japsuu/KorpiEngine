namespace KorpiEngine.Core.Threading.Pooling;

/// <summary>
/// Represents the execution priority of a job.
/// Higher priority (low numerical priority value) jobs are executed first.
/// </summary>
public static class WorkItemPriority
{
    /// <summary>
    /// The task should be executed as soon as possible.
    /// </summary>
    public const float HIGHEST = 0;
    
    /// <summary>
    /// A high priority.
    /// </summary>
    public const float HIGH = 25;
    
    /// <summary>
    /// The default priority.
    /// </summary>
    public const float NORMAL = 50;
    
    /// <summary>
    /// A low priority.
    /// </summary>
    public const float LOW = 75;
    
    /// <summary>
    /// The lowest priority.
    /// The task will be executed when all other tasks have been completed.
    /// </summary>
    public const float LOWEST = 100;
}
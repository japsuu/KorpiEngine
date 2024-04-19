namespace KorpiEngine.Core.Platform;

/// <summary>
/// Contains information about the current system.
/// </summary>
public static class SystemInfo
{
    /// <summary>
    /// Gets the number of processors available to the current process.
    /// </summary>
    public static int ProcessorCount { get; private set; }

    /// <summary>
    /// Id of the thread updating the main window.
    /// </summary>
    public static int MainThreadId { get; private set; }
    
    
    public static void Initialize()
    {
        ProcessorCount = Environment.ProcessorCount;
        MainThreadId = Environment.CurrentManagedThreadId;
    }
}
using System.Collections.Concurrent;
using KorpiEngine.Tools;

namespace KorpiEngine.Utils;

/// <summary>
/// A system that handles releasing memory.
/// </summary>
internal static class MemoryReleaseSystem
{
    // Thread-safe queue for IDisposable objects that need to be disposed on the main thread.
    private static readonly ConcurrentQueue<IDisposable> DisposeQueue = new();
    
    
    public static void Initialize()
    {
        Debug.Assert(Application.IsMainThread, "Memory release system can only be initialized on the main thread!");
    }
    
    /// <summary>
    /// Adds an object to the disposal queue.
    /// The object will be later disposed on the main thread.
    /// </summary>
    public static void AddToDisposeQueue(IDisposable obj)
    {
        DisposeQueue.Enqueue(obj);
    }
    
    
    [Profile]
    public static void ProcessDisposeQueue()
    {
        Debug.Assert(Application.IsMainThread, "Dispose queue can only be processed on the main thread!");
        
        while (DisposeQueue.TryDequeue(out IDisposable? obj))
        {
            obj.Dispose();
        }
    }
    
    
    public static void Shutdown()
    {
        Debug.Assert(Application.IsMainThread, "Memory release system can only be shut down on the main thread!");

        // Process all remaining objects in the queue
        ProcessDisposeQueue();
    }
}
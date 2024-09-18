namespace KorpiEngine.Utils;

/// <summary>
/// Base class for resources that support manual disposal.
/// This class implements the <see cref="IDisposable"/> interface and provides a safe way to dispose of both managed and unmanaged resources.
/// </summary>
public abstract class SafeDisposable : IDisposable
{
    /// <summary>
    /// True if this resource requires the main thread to dispose of it.
    /// False if it can be disposed of from any thread.
    /// </summary>
    protected abstract bool RequiresMainThreadDispose { get; }
    
    /// <summary>
    /// True if this resource has already been disposed of.
    /// </summary>
    protected bool IsDisposed { get; private set; }
    
    private bool _inDisposeQueue;


    /// <summary>
    /// Called by the garbage collector.
    /// A call to <see cref="Dispose"/> prevents this destructor from being called.
    /// </summary>
    ~SafeDisposable()
    {
        Dispose(false);
    }


    /// <summary>
    /// Releases all resources owned by this object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        // Take this object off the finalization queue to prevent the destructor from being called.
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Calls <see cref="DisposeResources"/> on the main thread,
    /// to release all resources owned by this object in a thread-safe manner.<br/><br/>
    /// 
    /// Can be overridden, but not recommended.
    /// Instead, override <see cref="DisposeResources"/>.<br/><br/>
    /// 
    /// Overriding implementations should call this base method to ensure proper disposal,
    /// and properly handle multiple calls to dispose (see <see cref="IsDisposed"/>).<br/><br/>
    ///
    /// Example implementation:
    /// <code>
    /// protected override void Dispose(bool manual)
    /// {
    ///     if (IsDisposed)
    ///         return;
    ///     base.Dispose(manual);
    ///     
    ///     if (manual)
    ///     {
    ///         // Dispose managed resources
    ///     }
    ///     
    ///     // Dispose unmanaged resources
    /// }
    /// </code>
    /// </summary>
    /// <param name="manual">True, if the call is performed explicitly by calling <see cref="Dispose"/>.
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread.
    /// Only unmanaged thread-safe resources can be disposed.</param>
    protected void Dispose(bool manual)
    {
        // Safely handle multiple calls to dispose
        if (IsDisposed)
            return;

        if (!manual && RequiresMainThreadDispose)
        {
            if (_inDisposeQueue)
                throw new InvalidOperationException("Dispose called from GC twice.");

            MemoryReleaseSystem.AddToDisposeQueue(this);
            _inDisposeQueue = true;
            return;
        }

        IsDisposed = true;
        _inDisposeQueue = false;
        DisposeResources();
    }


    /// <summary>
    /// Releases all resources owned by this object.
    /// Guaranteed to be called on the main thread.
    /// Guaranteed to be called only once.
    /// </summary>
    protected abstract void DisposeResources();
}
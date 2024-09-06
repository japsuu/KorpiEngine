namespace KorpiEngine.Utils;

/// <summary>
/// Base class for resources that require manual disposal.
/// This class implements the <see cref="IDisposable"/> interface and provides a safe way to dispose of both managed and unmanaged resources.
/// </summary>
public abstract class SafeDisposable : IDisposable
{
    /// <summary>
    /// True if this resource has already been disposed of.
    /// </summary>
    protected bool IsDisposed { get; private set; }


    /// <summary>
    /// Called by the garbage collector and an indicator for a resource leak because the manual Dispose() prevents this destructor from being called.
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
    /// Releases all owned resources.
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
    /// False, if caused by the GC and therefore from another thread and the result of a resource leak.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void Dispose(bool manual)
    {
        // Safely handle multiple calls to dispose
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}
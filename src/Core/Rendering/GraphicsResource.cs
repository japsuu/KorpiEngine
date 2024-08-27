using System.Reflection;
using KorpiEngine.Core.Rendering.Exceptions;

namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a graphics (GPU) resource.<br/>
/// Must be disposed explicitly, otherwise a warning will be logged indicating a memory leak.<br/>
/// Can be derived to inherit the dispose pattern.
/// </summary>
internal abstract class GraphicsResource : IDisposable
{
    /// <summary>
    /// True if this resource has already been disposed of.
    /// </summary>
    protected bool IsDisposed { get; private set; }


    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected GraphicsResource()
    {
    }


    /// <summary>
    /// Called by the garbage collector and an indicator for a resource leak because the manual Dispose() prevents this destructor from being called.
    /// </summary>
    ~GraphicsResource()
    {
        Dispose(false);
    }


    /// <summary>
    /// Releases all OpenGL handles related to this resource.
    /// </summary>
    public void Dispose()
    {
        // Dispose this resource
        Dispose(true);

        // Take this object off the finalization queue to prevent the destructor from being called
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases all OpenGL handles related to this resource.
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
    /// <param name="manual">True, if the call is performed explicitly within the OpenGL thread.
    /// Managed and unmanaged resources can be disposed).<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread and the result of a resource leak.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void Dispose(bool manual)
    {
        // Safely handle multiple calls to dispose
        if (IsDisposed)
            return;
        IsDisposed = true;
#if TOOLS
        if (!manual)
            throw new OpenGLException($"GraphicsResource of type {GetType().Name} was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif
    }


    /// <summary>
    /// Automatically calls <see cref="Dispose()"/> on all <see cref="GraphicsResource"/> objects found on the given object.
    /// </summary>
    /// <param name="obj"></param>
    internal static void DisposeAll(object obj)
    {
        // get all fields, including backing fields for properties
        foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            // check if it should be released
            if (!typeof(GraphicsResource).IsAssignableFrom(field.FieldType))
                continue;

            // and release it
            GraphicsResource? resource = (GraphicsResource?)field.GetValue(obj);
            resource?.Dispose();
        }
    }
}
using System.Reflection;
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a graphics (GPU) resource.<br/>
/// Must be disposed explicitly, otherwise a warning will be logged indicating a memory leak.<br/>
/// Can be derived to inherit the dispose pattern.
/// </summary>
public abstract class GraphicsResource : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsResource));

    /// <summary>
    /// Gets a values specifying if this resource has already been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected GraphicsResource()
    {
        IsDisposed = false;
    }


    /// <summary>
    /// Called by the garbage collector and an indicator for a resource leak because the manual dispose prevents this destructor from being called.
    /// </summary>
    ~GraphicsResource()
    {
        Logger.WarnFormat("GraphicsResource leaked: {0}", this);
        Dispose(false);
#if DEBUG
        throw new Exceptions.OpenGLException($"GraphicsResource leaked: {this}");
#endif
    }


    /// <summary>
    /// Releases all OpenGL handles related to this resource.
    /// </summary>
    public void Dispose()
    {
        // safely handle multiple calls to dispose
        if (IsDisposed) return;
        IsDisposed = true;

        // Dispose this resource
        Dispose(true);

        // prevent the destructor from being called
        GC.SuppressFinalize(this);

        // make sure the garbage collector does not eat our object before it is properly disposed
        GC.KeepAlive(this);
    }


    /// <summary>
    /// Releases all OpenGL handles related to this resource.
    /// </summary>
    /// <param name="manual">True if the call is performed explicitly and within the OpenGL thread, false if it is caused by the garbage collector and therefore from another thread and the result of a resource leak.</param>
    protected abstract void Dispose(bool manual);


    /// <summary>
    /// Automatically calls <see cref="Dispose()"/> on all <see cref="GraphicsResource"/> objects found on the given object.
    /// </summary>
    /// <param name="obj"></param>
    public static void DisposeAll(object obj)
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
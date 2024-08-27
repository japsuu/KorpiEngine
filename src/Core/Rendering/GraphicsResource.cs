using System.Reflection;
using KorpiEngine.Exceptions;

namespace KorpiEngine.Rendering;

/// <summary>
/// Represents a graphics (GPU) resource.<br/>
/// Must be disposed explicitly, otherwise a warning will be logged indicating a memory leak.<br/>
/// Can be derived to inherit the dispose pattern.
/// </summary>
internal abstract class GraphicsResource : SafeDisposable
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected GraphicsResource()
    {
    }


    protected override void Dispose(bool manual)
    {
        // Safely handle multiple calls to dispose
        if (IsDisposed)
            return;
        base.Dispose(manual);
        
#if TOOLS
        if (!manual)
            throw new ResourceLeakException($"GraphicsResource of type {GetType().Name} was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif
    }


    /// <summary>
    /// Automatically calls <see cref="Dispose"/> on all <see cref="GraphicsResource"/> objects found on the given object.
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
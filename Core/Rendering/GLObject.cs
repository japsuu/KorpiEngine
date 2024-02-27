namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents an OpenGL handle.<br/>
/// Must be disposed explicitly, otherwise there will be a memory leak which will be logged as a warning.
/// </summary>
public abstract class GLObject : GLResource, IEquatable<GLObject>
{
    /// <summary>
    /// The OpenGL handle.
    /// </summary>
    public readonly int Handle;


    /// <summary>
    /// Initializes a new instance of the GLResource class.
    /// </summary>
    protected GLObject(int handle)
    {
        Handle = handle;
    }


    public bool Equals(GLObject? other)
    {
        return other != null && Handle.Equals(other.Handle);
    }


    public override bool Equals(object? obj)
    {
        return obj is GLObject o && Equals(o);
    }


    public override int GetHashCode()
    {
        return Handle.GetHashCode();
    }


    public override string ToString()
    {
        return $"{GetType().Name}({Handle})";
    }
}
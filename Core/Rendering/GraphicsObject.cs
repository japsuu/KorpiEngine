namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a graphics resource handle.<br/>
/// Must be disposed explicitly, otherwise there will be a memory leak which will be logged as a warning.
/// </summary>
public abstract class GraphicsObject : GraphicsResource, IEquatable<GraphicsObject>
{
    /// <summary>
    /// The OpenGL handle.
    /// </summary>
    public readonly int Handle;


    /// <summary>
    /// Initializes a new instance of the GraphicsResource class.
    /// </summary>
    protected GraphicsObject(int handle)
    {
        Handle = handle;
    }


    public bool Equals(GraphicsObject? other)
    {
        return other != null && Handle.Equals(other.Handle);
    }


    public override bool Equals(object? obj)
    {
        return obj is GraphicsObject o && Equals(o);
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
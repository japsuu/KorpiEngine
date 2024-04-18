namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a shaderProgram object.
/// </summary>
public abstract class GraphicsProgram : GraphicsObject
{
    protected GraphicsProgram(int handle) : base(handle)
    {
    }
}
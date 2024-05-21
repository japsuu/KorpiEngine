namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a shaderProgram object.
/// </summary>
internal abstract class GraphicsProgram : GraphicsObject
{
    protected GraphicsProgram(int handle) : base(handle)
    {
    }
}
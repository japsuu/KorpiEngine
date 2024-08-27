namespace KorpiEngine.Core.Rendering;

internal abstract class GraphicsBuffer(int handle) : GraphicsObject(handle)
{
    internal abstract int SizeInBytes { get; }
}
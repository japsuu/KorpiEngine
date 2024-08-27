namespace KorpiEngine.Rendering;

internal abstract class GraphicsBuffer(int handle) : GraphicsObject(handle)
{
    internal abstract int SizeInBytes { get; }
}
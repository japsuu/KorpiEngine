namespace KorpiEngine.Core.Rendering;

internal abstract class GraphicsBuffer : GraphicsObject
{
    internal abstract int SizeInBytes { get; }
    
    
    protected GraphicsBuffer(int handle) : base(handle)
    {
    }
}
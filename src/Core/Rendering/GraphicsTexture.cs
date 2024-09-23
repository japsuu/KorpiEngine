namespace KorpiEngine.Rendering;

public abstract class GraphicsTexture(int handle) : GraphicsObject(handle)
{
    public abstract TextureType Type { get; protected set; }
}
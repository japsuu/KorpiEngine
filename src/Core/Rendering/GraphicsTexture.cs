using KorpiEngine.Rendering.Primitives;

namespace KorpiEngine.Rendering;

internal abstract class GraphicsTexture(int handle) : GraphicsObject(handle)
{
    public abstract TextureType Type { get; protected set; }
}
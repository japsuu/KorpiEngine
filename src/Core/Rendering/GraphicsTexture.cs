using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering;

internal abstract class GraphicsTexture(int handle) : GraphicsObject(handle)
{
    public abstract TextureType Type { get; protected set; }
}
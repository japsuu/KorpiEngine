using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Textures;

/// <summary>
/// Represents a layered texture.<br/>
/// Layered textures are all array, cube map and 3D textures.
/// </summary>
public abstract class LayeredTexture : Texture
{
    public override bool SupportsLayers => true;


    internal LayeredTexture(SizedInternalFormat internalFormat, int levels) : base(internalFormat, levels)
    {
    }
}
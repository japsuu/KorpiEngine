using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Textures;

/// <summary>
/// Represents a cubemap texture array.<br/>
/// Images in this texture are all cube maps. It contains multiple sets of cube maps, all within one texture.
/// The array length * 6 (number of cube faces) is part of the texture size.
/// </summary>
[Obsolete("Deprecated. Use the new GraphicsTexture pipeline instead", true)]
public sealed class TextureCubemapArray : LayeredTexture
{
    public override string Name { get; }
    public override TextureTarget TextureTarget => TextureTarget.TextureCubeMapArray;

    /// <summary>
    /// The size of the texture.<br/>
    /// This represents both width and height of the texture, because cube maps have to be square.
    /// </summary>
    public int Size { get; private set; }

    /// <summary>
    /// The number of layers.
    /// </summary>
    public int Layers { get; private set; }


    /// <summary>
    /// Allocates immutable texture storage with the given parameters.
    /// </summary>
    /// <param name="name">Name of the texture.</param>
    /// <param name="internalFormat">The internal format to allocate.</param>
    /// <param name="size">The width and height of the cube map faces.</param>
    /// <param name="layers">The number of layers to allocate.</param>
    /// <param name="levels">The number of mipmap levels.</param>
    public TextureCubemapArray(string name, SizedInternalFormat internalFormat, int size, int layers, int levels = 0) : base(internalFormat, GetLevels(levels, size))
    {
        Name = name;
        Size = size;
        Layers = layers;
        GL.BindTexture(TextureTarget, Handle);

        // note: the depth parameter is the number of layer-faces hence the multiplication by six,
        // see https://www.opengl.org/wiki/Texture_Storage#Immutable_storage
        GL.TexStorage3D((TextureTarget3d)TextureTarget, Levels, InternalFormat, Size, Size, 6 * Layers);
        CheckError();
    }
}
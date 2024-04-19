using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace KorpiEngine.Core.Rendering.Textures;

/// <summary>
/// Represents a 2D texture.<br/>
/// Images in this texture all are 2-dimensional. They have width and height, but no depth.
/// </summary>
[Obsolete("Deprecated. Use the new GraphicsTexture pipeline instead", true)]
public sealed class Texture2D : Texture
{
    public override string Name { get; }
    public override TextureTarget TextureTarget => TextureTarget.Texture2D;

    /// <summary>
    /// The width of the texture.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the texture.
    /// </summary>
    public int Height { get; private set; }


    /// <summary>
    /// Allocates immutable texture storage with the given parameters.<br/>
    /// A value of zero for the number of mipmap levels will default to the maximum number of levels possible for the given bitmaps width and height.
    /// </summary>
    /// <param name="name">Name of this texture</param>
    /// <param name="internalFormat">The internal format to allocate.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="levels">The number of mipmap levels.</param>
    public Texture2D(string name, SizedInternalFormat internalFormat, int width, int height, int levels = 0) :
        base(internalFormat, GetLevels(levels, width, height))
    {
        Name = name;
        Width = width;
        Height = height;
        GL.BindTexture(TextureTarget, Handle);
        GL.TexStorage2D((TextureTarget2d)TextureTarget, Levels, InternalFormat, Width, Height);
        CheckError();
    }    
    
    
    public static Texture2D LoadFromFile(string path, string texName)
    {
        Texture2D texture;
        
        // OpenGL has its texture origin in the lower left corner instead of the top left corner,
        // so we tell StbImageSharp to flip the image when loading.
        StbImage.stbi_set_flip_vertically_on_load(1);

        // Here we open a stream to the file and pass it to StbImageSharp to load.
        using (Stream stream = File.OpenRead(path))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            
            texture = new Texture2D(texName, SizedInternalFormat.Rgba8, image.Width, image.Height);

            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                image.Width,
                image.Height,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data);
        }

        texture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Nearest);
        texture.SetWrapMode(TextureWrapMode.Repeat);
        texture.SetParameter(TextureParameterName.TextureMaxAnisotropy, EngineConstants.ANISOTROPIC_FILTERING_LEVEL);
        texture.GenerateMipMaps();

        return texture;
    }


    /// <summary>
    /// Internal constructor used by <see cref="TextureFactory"/> to wrap a Texture2D instance around an already existing texture.
    /// </summary>
    internal Texture2D(string name, int textureHandle, SizedInternalFormat internalFormat, int width, int height, int levels) :
        base(textureHandle, internalFormat, levels)
    {
        Name = name;
        Width = width;
        Height = height;
    }
}
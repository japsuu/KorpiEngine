using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace KorpiEngine.Core.Rendering.Textures;

/// <summary>
/// Represents a cubemap texture.<br/>
/// There are exactly 6 distinct sets of 2D images, all of the same size. They act as 6 faces of a cube.
/// </summary>
[Obsolete("Deprecated. Use the new GraphicsTexture pipeline instead", true)]
public sealed class TextureCubemap : LayeredTexture
{
    public override string Name { get; }
    public override TextureTarget TextureTarget => TextureTarget.TextureCubeMap;

    /// <summary>
    /// The size of the texture.<br/>
    /// This represents both width and height of the texture, because cube maps have to be square.
    /// </summary>
    public int Size { get; private set; }


    /// <summary>
    /// Allocates immutable texture storage with the given parameters.
    /// </summary>
    /// <param name="name">Name of this texture.</param>
    /// <param name="internalFormat">The internal format to allocate.</param>
    /// <param name="size">The width and height of the cube map faces.</param>
    /// <param name="levels">The number of mipmap levels.</param>
    public TextureCubemap(string name, SizedInternalFormat internalFormat, int size, int levels = 0) : base(internalFormat, GetLevels(levels, size))
    {
        Name = name;
        Size = size;
        GL.BindTexture(TextureTarget, Handle);
        GL.TexStorage2D((TextureTarget2d)TextureTarget, Levels, InternalFormat, Size, Size);
        CheckError();
    }
    
    
    public static TextureCubemap LoadFromFile(string[] facesPaths, string texName)
    {
        if (facesPaths.Length != 6)
            throw new ArgumentException("Cubemap must have 6 textures.");

        TextureCubemap texture = null!;

        // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
        // so we tell StbImageSharp to flip the image when loading.
        StbImage.stbi_set_flip_vertically_on_load(1);

        for (int i = 0; i < 6; i++)
        {
            using Stream stream = File.OpenRead(facesPaths[i]);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
            
            if (image.Width != image.Height)
                throw new ArgumentException("Cubemap faces must be square.");
            
            if (i == 0)
                texture = new TextureCubemap(texName, SizedInternalFormat.Rgb8, image.Width, 1);

            GL.TexSubImage2D(
                TextureTarget.TextureCubeMapPositiveX + i,
                0,
                0,
                0,
                image.Width,
                image.Height,
                PixelFormat.Rgb,
                PixelType.UnsignedByte,
                image.Data);
        }

        texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        texture.SetWrapMode(TextureWrapMode.ClampToEdge);

        return texture;
    }
}
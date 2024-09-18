using ImageMagick;
using KorpiEngine.Rendering;

namespace KorpiEngine.AssetManagement;

public static class Texture2DLoader
{
    #region ImageMagick integration

    /// <summary>
    /// Creates a <see cref="Texture2D"/> from an <see cref="MagickImage"/>.
    /// </summary>
    /// <param name="image">The image to create the <see cref="Texture2D"/> with.</param>
    /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
    public static Texture2D FromImage(MagickImage image, bool generateMipmaps = false)
    {
        ArgumentNullException.ThrowIfNull(image);

        image.Flip();

        const TextureImageFormat format = TextureImageFormat.RGBA_16_UF;
        image.ColorSpace = ColorSpace.sRGB;
        image.ColorType = ColorType.TrueColorAlpha;

        IntPtr pixels = image.GetPixelsUnsafe().GetAreaPointer(0, 0, image.Width, image.Height);

        Texture2D texture = new(image.Width, image.Height, false, format);
        try
        {
            Graphics.Device.TexSubImage2D(texture.Handle, 0, 0, 0, image.Width, image.Height, pixels);

            if (generateMipmaps)
                texture.GenerateMipmaps();

            return texture;
        }
        catch
        {
            texture.DestroyImmediate();
            throw;
        }
    }


    /// <summary>
    /// Creates a <see cref="Texture2D"/> from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream from which to load an image.</param>
    /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
    public static Texture2D FromStream(Stream stream, bool generateMipmaps = false)
    {
        MagickImage image = new(stream);
        return FromImage(image, generateMipmaps);
    }


    /// <summary>
    /// Creates a <see cref="Texture2D"/> by loading an image from a file.
    /// </summary>
    /// <param name="file">The file containing the image to create the <see cref="Texture2D"/> with.</param>
    /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
    public static Texture2D FromFile(string file, bool generateMipmaps = false)
    {
        MagickImage image = new(file);
        return FromImage(image, generateMipmaps);
    }

    #endregion
}
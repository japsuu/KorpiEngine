using KorpiEngine.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.OpenGL;

/// <summary>
/// Represents a texture object.
/// </summary>
/// <remarks>
/// <code>
/// Type              Supports: Mipmaps Layered
/// -------------------------------------------
/// Texture1D                   yes
/// Texture2D                   yes
/// Texture3D                   yes     yes
/// Texture1DArray              yes     yes
/// Texture2DArray              yes     yes
/// TextureCubemap              yes     yes
/// TextureCubemapArray         yes     yes
/// Texture2DMultisample
/// Texture2DMultisampleArray           yes
/// TextureRectangle
/// TextureBuffer
/// </code>
/// </remarks>
internal sealed class GLTexture : GraphicsTexture
{
    public override TextureType Type { get; protected set; }

    public readonly TextureTarget Target;

    /// <summary>The internal format of the pixels.</summary>
    public readonly PixelInternalFormat InternalFormat;

    /// <summary>The data type of the components of the texture's pixels.</summary>
    public readonly PixelType PixelType;

    /// <summary>The format of the pixel data.</summary>
    public readonly PixelFormat PixelFormat;

    private static int? CurrentlyBoundHandle { get; set; }


    public GLTexture(TextureType type, TextureImageFormat format) : base(GL.GenTexture())
    {
        Type = type;
        Target = type switch
        {
            TextureType.Texture1D => TextureTarget.Texture1D,
            TextureType.Texture2D => TextureTarget.Texture2D,
            TextureType.Texture3D => TextureTarget.Texture3D,
            TextureType.TextureCubeMap => TextureTarget.TextureCubeMap,
            TextureType.Texture2DArray => TextureTarget.Texture2DArray,
            TextureType.Texture2DMultisample => TextureTarget.Texture2DMultisample,
            TextureType.Texture2DMultisampleArray => TextureTarget.Texture2DMultisampleArray,
            TextureType.Texture1DArray => TextureTarget.Texture1DArray,
            TextureType.TextureCubeMapArray => TextureTarget.TextureCubeMapArray,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        GetTextureFormatEnums(format, out InternalFormat, out PixelType, out PixelFormat);
    }


    public void Bind(bool force = true)
    {
        if (!force && CurrentlyBoundHandle == Handle)
            return;

        GL.BindTexture(Target, Handle);
        CurrentlyBoundHandle = Handle;
#if TOOLS
        Graphics.Device.TextureSwaps++;
#endif
    }


    public void GenerateMipmap()
    {
        if (Type == TextureType.Texture2DMultisample || Type == TextureType.Texture2DMultisampleArray)
            throw new InvalidOperationException("Cannot generate mipmaps for multisample textures");

        Bind(false);
        GL.GenerateMipmap((GenerateMipmapTarget)Target);
    }


    public void SetWrapS(TextureWrap wrap)
    {
        SetWrap(wrap, TextureParameterName.TextureWrapS);
    }


    public void SetWrapT(TextureWrap wrap)
    {
        SetWrap(wrap, TextureParameterName.TextureWrapT);
    }


    public void SetWrapR(TextureWrap wrap)
    {
        SetWrap(wrap, TextureParameterName.TextureWrapR);
    }


    public void SetTextureFilters(TextureMin min, TextureMag mag)
    {
        Bind(false);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)min);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)mag);
    }


    public void GetTexImage(int level, nint data)
    {
        Bind(false);
        GL.GetTexImage(Target, level, PixelFormat, PixelType, data);
    }


    public void TexImage2D(TextureTarget type, int mipLevel, int width, int height, int border, nint data)
    {
        Bind(false);
        GL.TexImage2D(type, mipLevel, InternalFormat, width, height, border, PixelFormat, PixelType, data);
    }


    public void TexImage3D(TextureTarget type, int mipLevel, int width, int height, int depth, int border, nint data)
    {
        Bind(false);
        GL.TexImage3D(type, mipLevel, InternalFormat, width, height, depth, border, PixelFormat, PixelType, data);
    }


    internal void TexSubImage2D(TextureTarget type, int mipLevel, int xOffset, int yOffset, int width, int height, nint data)
    {
        Bind(false);
        GL.TexSubImage2D(type, mipLevel, xOffset, yOffset, width, height, PixelFormat, PixelType, data);
    }


    internal void TexSubImage3D(TextureTarget type, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth, nint data)
    {
        Bind(false);
        GL.TexSubImage3D(type, mipLevel, xOffset, yOffset, zOffset, width, height, depth, PixelFormat, PixelType, data);
    }


    /// <summary>
    /// Turns a value from the <see cref="TextureImageFormat"/> enum into the necessary
    /// enums to create a texture's image/storage.
    /// </summary>
    /// <param name="imageFormat">The requested image format.</param>
    /// <param name="pixelInternalFormat">The pixel's internal format.</param>
    /// <param name="pixelType">The pixel's type.</param>
    /// <param name="pixelFormat">The pixel's format.</param>
    public static void GetTextureFormatEnums(TextureImageFormat imageFormat, out PixelInternalFormat pixelInternalFormat, out PixelType pixelType,
        out PixelFormat pixelFormat)
    {
        pixelType = imageFormat switch
        {
            TextureImageFormat.RGBA_8_UF => PixelType.UnsignedByte,
            TextureImageFormat.R_16_F => PixelType.Short,
            TextureImageFormat.RG_16_F => PixelType.Short,
            TextureImageFormat.RGB_16_F => PixelType.Short,
            TextureImageFormat.RGBA_16_F => PixelType.Short,
            TextureImageFormat.R_16_UF => PixelType.UnsignedShort,
            TextureImageFormat.RG_16_UF => PixelType.UnsignedShort,
            TextureImageFormat.RGB_16_UF => PixelType.UnsignedShort,
            TextureImageFormat.RGBA_16_UF => PixelType.UnsignedShort,
            TextureImageFormat.R_32_F => PixelType.Float,
            TextureImageFormat.RG_32_F => PixelType.Float,
            TextureImageFormat.RGB_32_F => PixelType.Float,
            TextureImageFormat.RGBA_32_F => PixelType.Float,
            TextureImageFormat.R_32_I => PixelType.Int,
            TextureImageFormat.RG_32_I => PixelType.Int,
            TextureImageFormat.RGB_32_I => PixelType.Int,
            TextureImageFormat.RGBA_32_I => PixelType.Int,
            TextureImageFormat.R_32_UI => PixelType.UnsignedInt,
            TextureImageFormat.RG_32_UI => PixelType.UnsignedInt,
            TextureImageFormat.RGB_32_UI => PixelType.UnsignedInt,
            TextureImageFormat.RGBA_32_UI => PixelType.UnsignedInt,
            TextureImageFormat.DEPTH_16 => PixelType.Float,
            TextureImageFormat.DEPTH_24 => PixelType.Float,
            TextureImageFormat.DEPTH_32_F => PixelType.Float,
            TextureImageFormat.DEPTH_24_STENCIL_8 => PixelType.UnsignedInt248,
            _ => throw new ArgumentException("Image format is not valid", nameof(imageFormat))
        };

        pixelInternalFormat = imageFormat switch
        {
            TextureImageFormat.RGBA_8_UF => PixelInternalFormat.Rgba8,
            TextureImageFormat.R_16_F => PixelInternalFormat.R16f,
            TextureImageFormat.RG_16_F => PixelInternalFormat.Rg16f,
            TextureImageFormat.RGB_16_F => PixelInternalFormat.Rgb16f,
            TextureImageFormat.RGBA_16_F => PixelInternalFormat.Rgba16f,
            TextureImageFormat.R_16_UF => PixelInternalFormat.R16f,
            TextureImageFormat.RG_16_UF => PixelInternalFormat.Rg16f,
            TextureImageFormat.RGB_16_UF => PixelInternalFormat.Rgb16f,
            TextureImageFormat.RGBA_16_UF => PixelInternalFormat.Rgba16f,
            TextureImageFormat.R_32_F => PixelInternalFormat.R32f,
            TextureImageFormat.RG_32_F => PixelInternalFormat.Rg32f,
            TextureImageFormat.RGB_32_F => PixelInternalFormat.Rgb32f,
            TextureImageFormat.RGBA_32_F => PixelInternalFormat.Rgba32f,
            TextureImageFormat.R_32_I => PixelInternalFormat.R32i,
            TextureImageFormat.RG_32_I => PixelInternalFormat.Rg32i,
            TextureImageFormat.RGB_32_I => PixelInternalFormat.Rgb32i,
            TextureImageFormat.RGBA_32_I => PixelInternalFormat.Rgba32i,
            TextureImageFormat.R_32_UI => PixelInternalFormat.R32ui,
            TextureImageFormat.RG_32_UI => PixelInternalFormat.Rg32ui,
            TextureImageFormat.RGB_32_UI => PixelInternalFormat.Rgb32ui,
            TextureImageFormat.RGBA_32_UI => PixelInternalFormat.Rgba32ui,
            TextureImageFormat.DEPTH_16 => PixelInternalFormat.DepthComponent16,
            TextureImageFormat.DEPTH_24 => PixelInternalFormat.DepthComponent24,
            TextureImageFormat.DEPTH_32_F => PixelInternalFormat.DepthComponent32f,
            TextureImageFormat.DEPTH_24_STENCIL_8 => PixelInternalFormat.Depth24Stencil8,
            _ => throw new ArgumentException("Image format is not valid", nameof(imageFormat))
        };

        pixelFormat = imageFormat switch
        {
            TextureImageFormat.RGBA_8_UF => PixelFormat.Rgba,
            TextureImageFormat.R_16_F => PixelFormat.Red,
            TextureImageFormat.RG_16_F => PixelFormat.Rg,
            TextureImageFormat.RGB_16_F => PixelFormat.Rgb,
            TextureImageFormat.RGBA_16_F => PixelFormat.Rgba,
            TextureImageFormat.R_16_UF => PixelFormat.Red,
            TextureImageFormat.RG_16_UF => PixelFormat.Rg,
            TextureImageFormat.RGB_16_UF => PixelFormat.Rgb,
            TextureImageFormat.RGBA_16_UF => PixelFormat.Rgba,
            TextureImageFormat.R_32_F => PixelFormat.Red,
            TextureImageFormat.RG_32_F => PixelFormat.Rg,
            TextureImageFormat.RGB_32_F => PixelFormat.Rgb,
            TextureImageFormat.RGBA_32_F => PixelFormat.Rgba,
            TextureImageFormat.R_32_I => PixelFormat.RgbaInteger,
            TextureImageFormat.RG_32_I => PixelFormat.RgInteger,
            TextureImageFormat.RGB_32_I => PixelFormat.RgbInteger,
            TextureImageFormat.RGBA_32_I => PixelFormat.RgbaInteger,
            TextureImageFormat.R_32_UI => PixelFormat.RedInteger,
            TextureImageFormat.RG_32_UI => PixelFormat.RgInteger,
            TextureImageFormat.RGB_32_UI => PixelFormat.RgbInteger,
            TextureImageFormat.RGBA_32_UI => PixelFormat.RgbaInteger,
            TextureImageFormat.DEPTH_16 => PixelFormat.DepthComponent,
            TextureImageFormat.DEPTH_24 => PixelFormat.DepthComponent,
            TextureImageFormat.DEPTH_32_F => PixelFormat.DepthComponent,
            TextureImageFormat.DEPTH_24_STENCIL_8 => PixelFormat.DepthStencil,
            _ => throw new ArgumentException("Image format is not valid", nameof(imageFormat))
        };
    }


    private void SetWrap(TextureWrap wrap, TextureParameterName wrapParamName)
    {
        Bind(false);
        GL.TexParameter(Target, wrapParamName, (int)wrap);
    }


    protected override void DisposeResources()
    {
        if (CurrentlyBoundHandle == Handle)
            CurrentlyBoundHandle = null;

        GL.DeleteTexture(Handle);
    }
}
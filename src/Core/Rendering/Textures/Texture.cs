using KorpiEngine.AssetManagement;

namespace KorpiEngine.Rendering;

public abstract class Texture : Asset
{
    private protected const TextureMin DEFAULT_MIN_FILTER = TextureMin.Nearest;
    private protected const TextureMin DEFAULT_MIPMAP_MIN_FILTER = TextureMin.NearestMipmapNearest;
    private protected const TextureMag DEFAULT_MAG_FILTER = TextureMag.Nearest;

    /// <summary>The handle for the GL Texture Object.</summary>
    internal readonly GraphicsTexture Handle;

    /// <summary>The type of this <see cref="Texture"/>, such as 1D, 2D, Multisampled 2D, Array 2D, CubeMap, etc.</summary>
    public readonly TextureType Type;

    public TextureMin MinFilter { get; protected set; }
    public TextureMag MagFilter { get; protected set; }
    public TextureWrap WrapMode { get; protected set; }

    /// <summary>The format for this <see cref="Texture"/>'s image.</summary>
    public readonly TextureImageFormat ImageFormat;

    /// <summary>Gets whether this <see cref="Texture"/> is mipmapped.</summary>
    public bool IsMipmapped { get; private set; }

    /// <summary>False if this <see cref="Texture"/> can be mipmapped (depends on a texture type).</summary>
    private readonly bool _isNotMipmappable;

    /// <summary>Gets whether this <see cref="Texture"/> can be mipmapped (depends on a texture type).</summary>
    public bool IsMipmappable => !_isNotMipmappable;


    /// <summary>
    /// Creates a <see cref="Texture"/> with specified <see cref="TextureType"/> and <see cref="TextureImageFormat"/>.
    /// </summary>
    /// <param name="type">The type of texture (or texture target) the texture will be.</param>
    /// <param name="imageFormat">The type of image format this texture will store.</param>
    private protected Texture(TextureType type, TextureImageFormat imageFormat) : base("New Texture")
    {
        if (!Enum.IsDefined(typeof(TextureType), type))
            throw new FormatException("Invalid texture target");

        if (!Enum.IsDefined(typeof(TextureImageFormat), imageFormat))
            throw new FormatException("Invalid texture image format");

        Type = type;
        ImageFormat = imageFormat;
        IsMipmapped = false;
        _isNotMipmappable = !IsTextureTypeMipmappable(type);
        Handle = Graphics.Device.CreateTexture(type, imageFormat);
        Graphics.Device.SetWrapS(Handle, TextureWrap.Repeat);
        Graphics.Device.SetWrapT(Handle, TextureWrap.Repeat);
        Graphics.Device.SetTextureFilters(Handle, DEFAULT_MIN_FILTER, DEFAULT_MAG_FILTER);
        MinFilter = DEFAULT_MIN_FILTER;
        MagFilter = DEFAULT_MAG_FILTER;
        WrapMode = TextureWrap.Repeat;
    }


    /// <summary>
    /// Sets this <see cref="Texture"/>'s minifying and magnifying filters.
    /// </summary>
    /// <param name="minFilter">The desired minifying filter for the <see cref="Texture"/>.</param>
    /// <param name="magFilter">The desired magnifying filter for the <see cref="Texture"/>.</param>
    public void SetTextureFilters(TextureMin minFilter, TextureMag magFilter)
    {
        Graphics.Device.SetTextureFilters(Handle, minFilter, magFilter);
        MinFilter = minFilter;
        MagFilter = magFilter;
    }


    /// <summary>
    /// Generates mipmaps for this <see cref="Texture"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public void GenerateMipmaps()
    {
        if (_isNotMipmappable)
            throw new InvalidOperationException(string.Concat("This texture type is not mipmappable! Type: ", Type.ToString()));

        Graphics.Device.GenerateMipmap(Handle);
        IsMipmapped = true;
    }


    protected override void OnDispose(bool manual)
    {
#if TOOLS
        if (!manual)
            throw new ResourceLeakException($"Texture '{Name}' of type {GetType().Name} was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif
        
        Handle.Dispose();
    }


    /// <summary>
    /// Gets whether the specified <see cref="TextureType"/> type is mipmappable.
    /// </summary>
    public static bool IsTextureTypeMipmappable(TextureType textureType) =>
        textureType is
            TextureType.Texture1D or
            TextureType.Texture2D or
            TextureType.Texture3D or
            TextureType.Texture1DArray or
            TextureType.Texture2DArray or
            TextureType.TextureCubeMap or
            TextureType.TextureCubeMapArray;
}
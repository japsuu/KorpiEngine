using KorpiEngine.AssetManagement;
using KorpiEngine.Tools.Serialization;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// A <see cref="Texture"/> whose image has two dimensions and support for multisampling.
/// </summary>
public sealed class Texture2D : Texture, ISerializable
{
    /// <summary>The width of this <see cref="Texture2D"/>.</summary>
    public int Width { get; private set; }

    /// <summary>The height of this <see cref="Texture2D"/>.</summary>
    public int Height { get; private set; }


    public Texture2D() : base(TextureType.Texture2D, TextureImageFormat.RGBA_8_UF)
    {
    }


    /// <summary>
    /// Creates a <see cref="Texture2D"/> with the desired parameters but no image data.
    /// </summary>
    /// <param name="width">The width of the <see cref="Texture2D"/>.</param>
    /// <param name="height">The height of the <see cref="Texture2D"/>.</param>
    /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture2D"/>.</param>
    /// <param name="imageFormat">The image format for this <see cref="Texture2D"/>.</param>
    public Texture2D(int width, int height, bool generateMipmaps = false, TextureImageFormat imageFormat = TextureImageFormat.RGBA_8_UF) : base(TextureType.Texture2D, imageFormat)
    {
        RecreateImage(width, height); //This also binds the texture

        if (generateMipmaps)
            GenerateMipmaps();

        Graphics.Device.SetTextureFilters(Handle, IsMipmapped ? DEFAULT_MIPMAP_MIN_FILTER : DEFAULT_MIN_FILTER, DEFAULT_MAG_FILTER);
        MinFilter = IsMipmapped ? DEFAULT_MIPMAP_MIN_FILTER : DEFAULT_MIN_FILTER;
        MagFilter = DEFAULT_MAG_FILTER;
    }


    public static AssetRef<Texture2D> Find(string path) => new(AssetManager.LoadAssetFile<Texture2D>(path, 0));


    /// <summary>
    /// Sets the data of an area of the <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="ptr">The pointer from which the pixel data will be read.</param>
    /// <param name="rectX">The X coordinate of the first pixel to write.</param>
    /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
    /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
    /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
    public void SetDataPtr(nint ptr, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

        Graphics.Device.TexSubImage2D(Handle, 0, rectX, rectY, rectWidth, rectHeight, ptr);
    }


    /// <summary>
    /// Sets the data of an area of the <see cref="Texture2D"/>.
    /// </summary>
    /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
    /// <param name="data">A <see cref="Memory{T}"/> containing the new pixel data.</param>
    /// <param name="rectX">The X coordinate of the first pixel to write.</param>
    /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
    /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
    /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
    public unsafe void SetData<T>(Memory<T> data, int rectX, int rectY, int rectWidth, int rectHeight) where T : unmanaged
    {
        ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);
        if (data.Length < rectWidth * rectHeight)
            throw new ArgumentException("Not enough pixel data", nameof(data));

        fixed (void* ptr = data.Span)
        {
            Graphics.Device.TexSubImage2D(Handle, 0, rectX, rectY, rectWidth, rectHeight, (nint)ptr);
        }
    }


    /// <summary>
    /// Sets the data of the entire <see cref="Texture2D"/>.
    /// </summary>
    /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
    /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
    public void SetData<T>(Memory<T> data) where T : unmanaged
    {
        SetData(data, 0, 0, Width, Height);
    }


    /// <summary>
    /// Gets the data of the entire <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="ptr">The pointer to which the pixel data will be written.</param>
    public void GetDataPtr(nint ptr)
    {
        Graphics.Device.GetTexImage(Handle, 0, ptr);
    }


    /// <summary>
    /// Gets the data of the entire <see cref="Texture2D"/>.
    /// </summary>
    /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
    /// <param name="data">A <see cref="Span{T}"/> in which to write the pixel data.</param>
    public unsafe void GetData<T>(Memory<T> data) where T : unmanaged
    {
        if (data.Length < Width * Height)
            throw new ArgumentException("Insufficient space to store the requested pixel data", nameof(data));

        fixed (void* ptr = data.Span)
        {
            Graphics.Device.GetTexImage(Handle, 0, (nint)ptr);
        }
    }


    public int GetSize()
    {
        int size = Width * Height;
        switch (ImageFormat)
        {
            // 32 bits per pixel
            case TextureImageFormat.R_32_UI:
            case TextureImageFormat.R_32_I:
            case TextureImageFormat.R_32_F:
            case TextureImageFormat.DEPTH_32_F:
            case TextureImageFormat.DEPTH_24_STENCIL_8:
                return size * 4;
            case TextureImageFormat.RG_32_UI:
            case TextureImageFormat.RG_32_I:
            case TextureImageFormat.RG_32_F:
                return size * 4 * 2;
            case TextureImageFormat.RGB_32_UI:
            case TextureImageFormat.RGB_32_I:
            case TextureImageFormat.RGB_32_F:
                return size * 4 * 3;
            case TextureImageFormat.RGBA_32_UI:
            case TextureImageFormat.RGBA_32_I:
            case TextureImageFormat.RGBA_32_F:
                return size * 4 * 4;
            
            // 24 bits per pixel
            case TextureImageFormat.DEPTH_24:
                return size * 3;
            
            // 16 bits per pixel
            case TextureImageFormat.R_16_F:
            case TextureImageFormat.R_16_UF:
            case TextureImageFormat.DEPTH_16:
                return size * 2 * 1;
            case TextureImageFormat.RG_16_F:
            case TextureImageFormat.RG_16_UF:
                return size * 2 * 2;
            case TextureImageFormat.RGB_16_F:
            case TextureImageFormat.RGB_16_UF:
                return size * 2 * 3;
            case TextureImageFormat.RGBA_16_F:
            case TextureImageFormat.RGBA_16_UF:
                return size * 2 * 4;
            
            // 8 bits per pixel
            case TextureImageFormat.RGBA_8_UF:
                return size;
            
            default:
                throw new InvalidOperationException("Invalid image format");
        }
    }


    /// <summary>
    /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
    /// </summary>
    /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
    /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate.</param>
    public void SetWrapModes(TextureWrap sWrapMode, TextureWrap tWrapMode)
    {
        Graphics.Device.SetWrapS(Handle, sWrapMode);
        Graphics.Device.SetWrapT(Handle, tWrapMode);
    }


    /// <summary>
    /// Recreates this <see cref="Texture2D"/>'s image with a new size,
    /// resizing the <see cref="Texture2D"/> but losing the image data.
    /// </summary>
    /// <param name="width">The new width for the <see cref="Texture2D"/>.</param>
    /// <param name="height">The new height for the <see cref="Texture2D"/>.</param>
    public void RecreateImage(int width, int height)
    {
        ValidateTextureSize(width, height);

        Width = width;
        Height = height;

        Graphics.Device.TexImage2D(Handle, 0, Width, Height, 0, 0);
    }


    private static void ValidateTextureSize(int width, int height)
    {
        if (width <= 0 || width > SystemInfo.MaxTextureSize)
            throw new ArgumentOutOfRangeException(nameof(width), width, $"{nameof(width)} must be in the range (0, {nameof(SystemInfo.MaxTextureSize)}({SystemInfo.MaxTextureSize})]");

        if (height <= 0 || height > SystemInfo.MaxTextureSize)
            throw new ArgumentOutOfRangeException(
                nameof(height), height, $"{nameof(height)} must be in the range (0, {nameof(SystemInfo.MaxTextureSize)}]");
    }


    private void ValidateRectOperation(int rectX, int rectY, int rectWidth, int rectHeight)
    {
        if (rectX < 0 || rectY >= Height)
            throw new ArgumentOutOfRangeException(nameof(rectX), rectX, $"{nameof(rectX)} must be in the range [0, {nameof(Width)})");

        if (rectY < 0 || rectY >= Height)
            throw new ArgumentOutOfRangeException(nameof(rectY), rectY, $"{nameof(rectY)} must be in the range [0, {nameof(Height)})");

        if (rectWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, $"{nameof(rectWidth)} must be greater than 0");

        if (rectHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, $"{nameof(rectHeight)}must be greater than 0");

        if (rectWidth > Width - rectX || rectHeight > Height - rectY)
            throw new InvalidOperationException("Specified area is outside of the texture's storage");
    }


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();
        compoundTag.Add("Width", new SerializedProperty(Width));
        compoundTag.Add("Height", new SerializedProperty(Height));
        compoundTag.Add("IsMipMapped", new SerializedProperty(IsMipmapped));
        compoundTag.Add("ImageFormat", new SerializedProperty((int)ImageFormat));
        compoundTag.Add("MinFilter", new SerializedProperty((int)MinFilter));
        compoundTag.Add("MagFilter", new SerializedProperty((int)MagFilter));
        compoundTag.Add("Wrap", new SerializedProperty((int)WrapMode));
        Memory<byte> memory = new byte[GetSize()];
        GetData(memory);
        compoundTag.Add("Data", new SerializedProperty(memory.ToArray()));

        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        Width = value["Width"].IntValue;
        Height = value["Height"].IntValue;
        bool isMipMapped = value["IsMipMapped"].BoolValue;
        TextureImageFormat imageFormat = (TextureImageFormat)value["ImageFormat"].IntValue;
        TextureMin minFilter = (TextureMin)value["MinFilter"].IntValue;
        TextureMag magFilter = (TextureMag)value["MagFilter"].IntValue;
        TextureWrap wrap = (TextureWrap)value["Wrap"].IntValue;

        Type[] param =
        [
            typeof(uint),
            typeof(uint),
            typeof(bool),
            typeof(TextureImageFormat)
        ];
        object[] values =
        [
            Width,
            Height,
            false,
            imageFormat
        ];
        typeof(Texture2D).GetConstructor(param)!.Invoke(this, values);

        Memory<byte> memory = value["Data"].ByteArrayValue;
        SetData(memory);

        if (isMipMapped)
            GenerateMipmaps();

        SetTextureFilters(minFilter, magFilter);
        SetWrapModes(wrap, wrap);
    }
}
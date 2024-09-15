using KorpiEngine.Rendering;

namespace KorpiEngine.AssetManagement;

[AssetImporter(".png", ".bmp", ".jpg", ".jpeg", ".qoi", ".psd", ".tga", ".dds", ".hdr", ".ktx", ".pkm", ".pvr")]
internal class TextureImporter : AssetImporter
{
    public bool GenerateMipmaps { get; set; } = true;
    public TextureWrap TextureWrap { get; set; } = TextureWrap.Repeat;
    public TextureMin TextureMinFilter { get; set; } = TextureMin.LinearMipmapLinear;
    public TextureMag TextureMagFilter { get; set; } = TextureMag.Linear;

    
    public override void Import(AssetImportContext context)
    {
        // Load the Texture into a TextureData Object and serialize to Asset Folder
        Texture2D texture = Texture2DLoader.FromFile(context.FilePath.FullName);

        texture.SetTextureFilters(TextureMinFilter, TextureMagFilter);
        texture.SetWrapModes(TextureWrap, TextureWrap);

        if (GenerateMipmaps)
            texture.GenerateMipmaps();

        context.SetMainAsset(texture);
    }
}
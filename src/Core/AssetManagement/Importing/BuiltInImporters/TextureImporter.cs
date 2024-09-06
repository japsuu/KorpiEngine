using KorpiEngine.Rendering.Primitives;
using KorpiEngine.Rendering.Textures;

namespace KorpiEngine.AssetManagement.Importers;

[AssetImporter(".png", ".bmp", ".jpg", ".jpeg", ".qoi", ".psd", ".tga", ".dds", ".hdr", ".ktx", ".pkm", ".pvr")]
internal class TextureImporter : AssetImporter
{
    public bool GenerateMipmaps { get; set; } = true;
    public TextureWrap TextureWrap { get; set; } = TextureWrap.Repeat;
    public TextureMin TextureMinFilter { get; set; } = TextureMin.LinearMipmapLinear;
    public TextureMag TextureMagFilter { get; set; } = TextureMag.Linear;

    public override AssetInstance Import(FileInfo assetPath)
    {
        // Load the Texture into a TextureData Object and serialize to Asset Folder
        Texture2D texture = Texture2DLoader.FromFile(assetPath.FullName);

        texture.SetTextureFilters(TextureMinFilter, TextureMagFilter);
        texture.SetWrapModes(TextureWrap, TextureWrap);

        if (GenerateMipmaps)
            texture.GenerateMipmaps();

        return texture;
    }
}
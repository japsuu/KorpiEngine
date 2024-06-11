using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Internal.AssetManagement.Importers;

[AssetImporter(".png", ".bmp", ".jpg", ".jpeg", ".qoi", ".psd", ".tga", ".dds", ".hdr", ".ktx", ".pkm", ".pvr")]
internal class TextureImporter : AssetImporter
{
    public bool GenerateMipmaps = true;
    public TextureWrap TextureWrap = TextureWrap.Repeat;
    public TextureMin TextureMinFilter = TextureMin.LinearMipmapLinear;
    public TextureMag TextureMagFilter = TextureMag.Linear;

    public override EngineObject Import(FileInfo assetPath)
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
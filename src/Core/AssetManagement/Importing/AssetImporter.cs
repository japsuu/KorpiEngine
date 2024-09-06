namespace KorpiEngine.AssetManagement;

public abstract class AssetImporter
{
    public abstract AssetInstance? Import(FileInfo assetPath);
}
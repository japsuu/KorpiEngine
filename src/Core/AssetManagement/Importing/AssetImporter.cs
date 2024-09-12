namespace KorpiEngine.AssetManagement;

public abstract class AssetImporter
{
    public abstract Asset? Import(FileInfo assetPath);
}
namespace KorpiEngine.AssetManagement;

public abstract class AssetImporter
{
    public abstract Resource? Import(FileInfo assetPath);
}
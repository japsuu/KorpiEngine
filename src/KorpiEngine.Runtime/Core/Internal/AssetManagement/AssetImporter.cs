namespace KorpiEngine.Core.Internal.AssetManagement;

public abstract class AssetImporter
{
    public abstract Resource? Import(FileInfo assetPath);
}
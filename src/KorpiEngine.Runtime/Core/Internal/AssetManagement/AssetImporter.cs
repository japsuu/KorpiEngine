namespace KorpiEngine.Core.API.AssetManagement;

public abstract class AssetImporter
{
    public abstract EngineObject? Import(FileInfo assetPath);
}
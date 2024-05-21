namespace KorpiEngine.Core.Internal.AssetManagement;

public abstract class AssetImporter
{
    public abstract EngineObject? Import(FileInfo assetPath);
}
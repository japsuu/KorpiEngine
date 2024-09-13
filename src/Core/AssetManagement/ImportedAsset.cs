using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ImportedAsset
{
    private readonly Asset _mainAsset;
    private readonly IReadOnlyList<Asset> _subAssets;
    
    public readonly UUID AssetID;
    public readonly FileInfo AssetPath;


    public ImportedAsset(AssetImportContext context)
    {
        _mainAsset = context.MainAsset ?? throw new InvalidOperationException("Main asset not set in import context.");
        _subAssets = context.SubAssets;
        
        AssetID = context.AssetID;
        AssetPath = context.FilePath;
    }
    
    
    public Asset GetAsset(ushort subID)
    {
        if (subID == 0)
            return _mainAsset;
        
        if (subID > _subAssets.Count)
            throw new ArgumentOutOfRangeException(nameof(subID), "SubID out of range.");
        
        return _subAssets[subID - 1];
    }
    
    
    public void Destroy()
    {
        _mainAsset.DestroyImmediate();
        
        foreach (Asset asset in _subAssets)
            asset.DestroyImmediate();
    }
}
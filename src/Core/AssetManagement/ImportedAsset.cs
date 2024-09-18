using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ImportedAsset
{
    private readonly Asset _mainAsset;
    private readonly IReadOnlyList<Asset> _subAssets;
    private readonly IReadOnlyList<AssetRef<Asset>> _dependencies;
    
    public readonly UUID AssetID;
    public readonly string RelativeAssetPath;


    public ImportedAsset(AssetImportContext context)
    {
        _mainAsset = context.MainAsset ?? throw new InvalidOperationException("Main asset not set in import context.");
        _subAssets = context.SubAssets;
        _dependencies = context.Dependencies;
        
        AssetID = context.AssetID;
        RelativeAssetPath = context.RelativeAssetPath;
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
        Asset.AllowManualExternalDestroy = true;
        
        // Destroy the main asset
        _mainAsset.DestroyImmediate();
        
        // Destroy sub-assets
        foreach (Asset asset in _subAssets)
            asset.DestroyImmediate();
        
        Asset.AllowManualExternalDestroy = false;
        
        // Release dependencies
        foreach (AssetRef<Asset> dependency in _dependencies)
            dependency.Release();
    }
}
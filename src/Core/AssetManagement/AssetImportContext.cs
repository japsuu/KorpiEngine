using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
public class AssetImportContext(FileInfo filePath, UUID assetID)
{
    private readonly List<Asset> _subAssets = [];
    private Asset? _mainAsset;
    
    internal IReadOnlyList<Asset> SubAssets => _subAssets;
    internal Asset? MainAsset => _mainAsset;
    
    public readonly FileInfo FilePath = filePath;
    public readonly UUID AssetID = assetID;


    public void SetMainAsset(Asset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        
        if (_mainAsset != null)
            throw new InvalidOperationException("The main asset has already been set.");

        if (_subAssets.Contains(asset))
            throw new InvalidOperationException($"The asset {asset} is already a sub-asset in this context.");

        // The main asset has a subID of 0
        asset.SetExternalInfo(AssetID, 0);
        
        _mainAsset = asset;
    }
    
    
    public AssetRef<T> AddSubAsset<T>(T asset) where T : Asset
    {
        ArgumentNullException.ThrowIfNull(asset);

        if (_subAssets.Contains(asset))
            throw new InvalidOperationException($"The asset {asset} is already a sub-asset in this context.");

        if (ReferenceEquals(asset, _mainAsset))
            throw new InvalidOperationException($"The asset {asset} is already the main asset in this context.");

        ushort subID = (ushort)(_subAssets.Count + 1);
        asset.SetExternalInfo(AssetID, subID);
        
        _subAssets.Add(asset);
        
        return new AssetRef<T>(asset, subID);
    }
}
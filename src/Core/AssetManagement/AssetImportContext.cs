using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
public class AssetImportContext(FileInfo filePath, UUID assetID)
{
    private readonly List<AssetRef<Asset>> _dependencies = [];
    private readonly List<Asset> _subAssets = [];
    private Asset? _mainAsset;
    
    internal IReadOnlyList<AssetRef<Asset>> Dependencies => _dependencies;
    internal IReadOnlyList<Asset> SubAssets => _subAssets;
    internal Asset? MainAsset => _mainAsset;
    
    public readonly FileInfo FilePath = filePath;
    public readonly UUID AssetID = assetID;


    /// <summary>
    /// Sets the main asset of this import context.
    /// </summary>
    /// <param name="asset">The main asset to set.</param>
    /// <exception cref="InvalidOperationException">Thrown if the main asset has already been set.</exception>
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
    
    
    /// <summary>
    /// Adds a sub-asset to this import context.
    /// Sub-assets are assets that are part of the main asset, but are not the main asset itself.
    /// These are unloaded when the main asset is unloaded.
    /// </summary>
    /// <param name="asset">The sub-asset to add.</param>
    /// <typeparam name="T">The type of the sub-asset to add.</typeparam>
    /// <returns>The AssetRef to the added sub-asset.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the asset is already a sub-asset in this context.</exception>
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
        
        return new AssetRef<T>(asset);
    }
    
    
    public void AddDependency<T>(T asset) where T : Asset
    {
        ArgumentNullException.ThrowIfNull(asset);
        
        if (_subAssets.Contains(asset))
            throw new InvalidOperationException($"The asset {asset} is already a sub-asset in this context.");

        if (ReferenceEquals(asset, _mainAsset))
            throw new InvalidOperationException($"The asset {asset} is already the main asset in this context.");

        AssetRef<Asset> assetRef = new(asset);
        _dependencies.Add(assetRef);
    }
}
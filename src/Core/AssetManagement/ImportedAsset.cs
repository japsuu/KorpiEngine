﻿using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ImportedAsset
{
    public readonly UUID AssetID;
    public readonly FileInfo AssetPath;
    public readonly AssetInstance Instance;


    public ImportedAsset(UUID assetID, FileInfo assetPath, AssetInstance instance)
    {
        AssetID = assetID;
        AssetPath = assetPath;
        Instance = instance;
    }
    
    
    public void Destroy()
    {
        Instance.DestroyImmediate();
    }
}
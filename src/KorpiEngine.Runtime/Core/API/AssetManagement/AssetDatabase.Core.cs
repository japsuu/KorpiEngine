﻿using KorpiEngine.Core.Internal.AssetManagement;

namespace KorpiEngine.Core.API.AssetManagement;

public static partial class AssetDatabase
{
    /// <summary>
    /// Relative path of an asset to its GUID.
    /// The path is relative to the project root.
    /// </summary>
    private static readonly Dictionary<string, Guid> RelativePathToGuid = new();
    private static readonly Dictionary<Guid, Asset> GuidToAsset = new();


    #region ASSET LOADING

    /// <summary>
    /// Loads an asset of the specified type from the specified file path.
    /// Use this when the asset GUID is not known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to load.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T? LoadAsset<T>(string relativeAssetPath) where T : Resource
    {
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        // Check if the asset at the specified path has been loaded before
        if (TryGetGuidFromPath(fileInfo, out Guid guid))
            return LoadAsset<T>(guid);

        // The path hasn't been accessed before, so we need to import the asset
        return Import(fileInfo)?.Instance as T;
    }


    /// <summary>
    /// Loads an asset of the specified type with the specified known GUID.
    /// Use this when the asset GUID is known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetGuid">The GUID of the asset to load.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T? LoadAsset<T>(Guid assetGuid) where T : Resource
    {
        if (assetGuid == Guid.Empty)
            throw new ArgumentException("Asset Guid cannot be empty", nameof(assetGuid));

        try
        {
            return GetAssetWithId(assetGuid)?.Instance as T;
        }
        catch (Exception e)
        {
            Application.Logger.Error($"Failed to load asset with GUID {assetGuid}. Reason: {e.Message}");
            return null;
        }
    }

    #endregion


    #region ASSET UNLOADING

    public static void UnloadAsset(Guid guid)
    {
        if (!GuidToAsset.TryGetValue(guid, out Asset? asset))
            return;

        string relativePath = ToRelativePath(asset.AssetPath);
        
        asset.Destroy();
        GuidToAsset.Remove(guid);
        RelativePathToGuid.Remove(relativePath);
    }

    #endregion


    /// <summary>
    /// Reimports an asset from the specified file.
    /// </summary>
    /// <param name="assetFile">The asset file to reimport.</param>
    private static Asset? Import(FileInfo assetFile)
    {
        string relativePath = ToRelativePath(assetFile);
        
        Application.Logger.Info($"Attempting to Import {relativePath}...");
        ArgumentNullException.ThrowIfNull(assetFile);

        // Make sure the path exists
        if (!File.Exists(assetFile.FullName))
        {
            Application.Logger.Error($"Failed to import {relativePath}. Asset does not exist.");
            return null;
        }
        
        // Make sure the file extension of the asset file is supported
        if (!AssetImporterAttribute.SupportsExtension(assetFile.Extension))
        {
            string supportedExtensions = AssetImporterAttribute.GetSupportedExtensions();
            Application.Logger.Error($"Cannot import {relativePath}. Unsupported file extension. Supported extensions are: '{supportedExtensions}'.");
            return null;
        }

        try
        {
            // Get the importer for the asset
            Type? importerType = AssetImporterAttribute.GetImporter(assetFile.Extension);
            if (importerType is null)
                throw new Exception($"No importer found for asset with extension {assetFile.Extension}");
        
            AssetImporter? importer = (AssetImporter?)Activator.CreateInstance(importerType);
            Resource? instance = importer?.Import(assetFile);
            
            if (instance == null)
                throw new Exception("The importer failed.");
            
            // Generate a new GUID for the asset
            Guid assetID = Guid.NewGuid();
            instance.AssetID = assetID;
            Asset asset = new(assetID, assetFile, instance);

            RelativePathToGuid[relativePath] = assetID;
            GuidToAsset[assetID] = asset;
            
            Application.Logger.Info($"Successfully imported {relativePath}");
            return asset;
        }
        catch (Exception e)
        {
            Application.Logger.Error($"Failed to import the asset {assetFile.FullName}. Reason: {e.Message}");
            return null;
        }
    }


    /// <summary>
    /// Gets the asset with the specified GUID.
    /// </summary>
    private static Asset? GetAssetWithId(Guid assetGuid) => GuidToAsset.GetValueOrDefault(assetGuid);
}
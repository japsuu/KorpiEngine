
# Working With External Assets

External assets are files that are loaded at runtime, such as textures, models, and audio files.

Any files inside the `Assets` directory (located at the root of the project) are considered external assets, and are copied to the build output directory. Make sure to distribute them with your game.

## Loading/Unloading Assets

Assets can be loaded/unloaded at runtime using the `AssetManager` API (see @KorpiEngine.AssetManagement.AssetManager), which makes sure that each asset is loaded only once, and that it is properly disposed of when no longer needed.

Example (loading the default albedo texture):
```csharp
string relativePath = "Assets/Defaults/default_albedo.png";
AssetRef<Texture2D> albedoTex = AssetManager.LoadAssetFile<Texture2D>(relativePath);
AssetManager.UnloadAsset(albedoTex.AssetID);
```

Take note of how the returned asset is wrapped in a `AssetRef` object. This is required to, for example, support dynamic asset unloading. Read more about it in the next section.

## Referencing Assets

When storing a reference to any class that inherits `AssetInstance`, it is recommended to wrap the reference in an `AssetRef` object.

Its purpose is to reference Assets abstractly based on their actual path. It is an "indirection layer" that simplifies dealing with certain use cases: While actual AssetInstances may be unavailable, not-yet-loaded, disposed or disposed-and-then-reloaded, their AssetRefs always remain comparable and reliable. You can think of it as a smart reference that knows what it should contain and takes care of actually doing so.

Also, storing a direct reference to an asset would prevent it from being collected by the GC and unloaded from memory. 'AssetRef' internally uses weak references to avoid this issue.

## Asset Importers

Asset importers are used to load external assets (files) into memory.
When the user tries to load an asset, the engine will look for an importer that can handle the file extension. If one is found, the importer will be used to load the asset.

 You can also create custom importers for your own file types.

### Natively Supported File Types

The engine comes with built-in importers for common file types, such as textures, shaders, and models.
The following file types are supported out of the box:
- Textures: PNG, BMP, JPG, JPEG, QOI, PSD, TGA, DDS, HDR, KTX, PKM, PVR
- Shaders: [KSL (Korpi Shader Language)](shaders.md)
- Models: OBJ, BLEND, DAE, FBX, GLTF, PLY, PMX, STL

### Custom Importers

To create a custom importer, you need to implement the `AssetImporter` abstract class,
decorate it with the `AssetImporter` attribute, and specify the file extensions it supports.
```csharp
[AssetImporter(".png", ".jpg")]
public class ExampleTextureImporter : AssetImporter
{
    public bool GenerateMipMaps { get; set; } = true;

    public override AssetInstance Import(FileInfo assetPath)
    {
        // File at assetPath is guaranteed to exist
        Texture2D texture = ImportTextureFromFile(assetPath);

        if (GenerateMipMaps)
            texture.GenerateMipMaps();

        return texture;
    }
    
    private Texture2D ImportTextureFromFile(FileInfo assetPath)
    {
        // Load the texture from the file
        // ...
    }
}
```
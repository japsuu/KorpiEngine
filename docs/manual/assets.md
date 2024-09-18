
# Working With Assets

There are two types of assets: _runtime assets_ and _external assets_.
- _Runtime assets_ are **created** at runtime, such as render textures and dynamic meshes.
- _External assets_ are **loaded** at runtime from files, such as textures, models, and audio files.

> [!NOTE]
> Please note that all assets (both runtime and external) inherit from the same `Asset` class, which provides a common interface for managing assets. `Asset` provides the `IsExternal` and `ExternalInfo` properties to distinguish between runtime and external assets.

<br/>

## Loading/Unloading External Assets

> Any files inside the `Assets` directory (located at the root of the project) are considered external assets, and are copied to the build output directory. Make sure to distribute them with your game.

Assets can be loaded/unloaded at runtime using the `AssetProvider` API (see <xref:KorpiEngine.AssetManagement.AssetProvider>) provided by `Application.AssetProvider`, which makes sure that each asset is loaded only once.

Asset unloading is not covered here yet, since unloading assets manually is usually not necessary nor recommended. If you know what you're doing, go for it.

Example (loading a texture):
```csharp
string relativePath = "Assets/texture.png";
AssetRef<Texture2D> tex = Application.AssetProvider.LoadAsset<Texture2D>(relativePath);
// Use the texture
```

> [!NOTE]
> Take note of how the returned asset is wrapped in a `AssetRef` object. This is required to, for example, support dynamic asset unloading. Read more about it in the next section.

<br/>

## Referencing Assets

When storing a reference to any class that inherits `AssetInstance`, it is recommended to wrap the reference in an `AssetRef` object.

Its purpose is to reference Assets abstractly based on their actual path. It is an "indirection layer" that simplifies dealing with certain use cases: While actual AssetInstances may be unavailable, not-yet-loaded or destroyed, their AssetRefs always remain comparable and reliable. You can think of it as a smart reference that knows what it should contain and takes care of actually doing so.

<br/>

## Asset Importers

Asset importers are used to load external assets (files) into memory.
When the user tries to load an asset, the engine will look for an importer that can handle the file extension. If one is found, the importer will be used to load the asset.

You can also create custom importers for your own file types.

<br/>

### Natively Supported File Types

The engine comes with built-in importers for common file types, such as textures, shaders, and models.
The following file types are supported out of the box:
- Textures: PNG, BMP, JPG, JPEG, QOI, PSD, TGA, DDS, HDR, KTX, PKM, PVR
- Shaders: [KSL (Korpi Shader Language)](shaders.md)
- Models: OBJ, BLEND, DAE, FBX, GLTF, PLY, PMX, STL

<br/>

### Custom Importers

To create a custom importer, you need to implement the `AssetImporter` abstract class,
decorate it with the `AssetImporter` attribute, and specify the file extensions it supports.
```csharp
[AssetImporter(".png", ".jpg")]
public class ExampleTextureImporter : AssetImporter
{
    public bool GenerateMipMaps { get; set; } = true;

    public override void Import(AssetImportContext context)
    {
        // File at context.FilePath is guaranteed to exist
        Texture2D texture = ImportTextureFromFile(context.FilePath);

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
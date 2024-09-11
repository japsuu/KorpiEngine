
# Working With Assets

There are two types of assets: _runtime assets_ and _external assets_.
- _Runtime assets_ are **created** at runtime, such as render textures and dynamic meshes.
- _External assets_ are **loaded** at runtime from files, such as textures, models, and audio files.

> [!NOTE]
> Please note that all assets (both runtime and external) inherit from the `AssetInstance` class, which provides a common interface for managing assets. `AssetInstance` provides the `IsRuntime` property to distinguish between runtime and external assets.

<br/>

## Loading/Unloading External Assets

> Any files inside the `Assets` directory (located at the root of the project) are considered external assets, and are copied to the build output directory. Make sure to distribute them with your game.

External assets can be loaded at runtime using the `AssetManager` API (see <xref:KorpiEngine.AssetManagement.AssetManager>), which makes sure that each asset is loaded only once, and that it is properly disposed of when no longer needed.
- Asset load and release are working in pairs. For each call to `AssetManager.Load()`, a corresponding call to `AssetInstance.Release()` is expected.
- An asset is actually loaded only during the first call to `AssetManager.Load()`. All later calls only result in an asset reference increment.
- An asset is actually unloaded only when the asset has no references left.

The `AssetManager.Get()` method can be used, if you temporarily need the asset and do not want to increase its reference count.

### Example (loading a texture):
```csharp
Texture2D texture = AssetManager.LoadAssetFile<Texture2D>("Assets/texture.png");

// Use the texture

texture.Release();
```

> [!WARNING]
> Always make sure to call `Release()` on the asset when you are done using it. Failing to do so will result in memory leaks.

<br/>

## ExternalAssetRef

`ExternalAssetRef`'s purpose is to reference Assets abstractly based on their actual path.
It is an "indirection layer" that simplifies dealing with certain use cases: While actual AssetInstances may be unavailable, not-yet-loaded, disposed or disposed-and-then-reloaded, their AssetRefs always remain comparable and reliable.
You can think of it as a smart reference that knows what it should contain and takes care of actually doing so.

## Referencing Assets and Ownership

---------------------------------------------------------------------------

Classes/Objects should never expose their owned `AssetReference` objects publicly. Instead, they should expose the `Asset` objects themselves.
This is done to prevent the outside world from accidentally calling `.Release()` on the `AssetReference` object, which would result in the asset being unloaded while it is still potentially in use by the original owner.
This allows other classes to take the `Asset` and create their own `AssetReference` objects.

---------------------------------------------------------------------------

Generally, the object that created/loaded an `AssetInstance` also owns it, and should be responsible for calling `.Release()` on it when it is no longer needed. This means that if the asset is passed to another object, the other object should not call `.Release()` on it.

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
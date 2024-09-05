
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
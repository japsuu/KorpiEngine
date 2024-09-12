using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public static class AssetExtensions
{
    public static AssetReference<T> CreateReference<T>(this T asset) where T : Asset
    {
        asset.IncreaseReferenceCount();
        return AssetReference<T>.Create(asset);
    }
    
    
    /// <summary>
    /// Explicitly increases the reference count of the asset, without creating a reference.
    /// </summary>
    /// <param name="asset">The asset to increase the reference count of.</param>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <returns>The asset with the reference count increased.</returns>
    private static T IncreaseReferenceCount<T>(this T asset) where T : Asset
    {
        asset.ReferenceCount++;
        return asset;
    }
    
    
    /// <summary>
    /// Explicitly decreases the reference count of the asset, without creating a reference.
    /// </summary>
    /// <param name="asset">The asset to decrease the reference count of.</param>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <returns>The asset with the reference count decreased.</returns>
    private static T DecreaseReferenceCount<T>(this T asset) where T : Asset
    {
        asset.ReferenceCount--;
        return asset;
    }


    internal static bool TryRelease<T>(this AssetReference<T> reference) where T : Asset
    {
        T? asset = reference.Asset;
        if (asset == null)
            return false;

        asset.DecreaseReferenceCount();
        return asset.ReferenceCount <= 0;
    }
}

/// <summary>
/// Represents a reference to an asset that keeps track of how many times it is referenced.
/// Automatically increments the reference count when created and decrements it when disposed.
/// When the reference count reaches 0, the asset is unloaded (external assets) or disposed (runtime assets).
/// </summary>
/// <typeparam name="T">The type of the asset to reference.</typeparam>
public sealed class AssetReference<T> : SafeDisposable, IEquatable<AssetReference<T>> where T : Asset
{
    /// <summary>
    /// The referenced <see cref="AssetManagement.Asset"/>.
    /// Null, if the <see cref="AssetReference{T}"/> has been disposed.
    /// </summary>
    public T? Asset { get; private set; }
    
    public bool HasBeenReleased { get; private set; }
    
    public bool IsAlive => Asset != null;

    /// <summary>
    /// Returns whether the referenced asset is loaded from a file.
    /// </summary>
    public bool IsAssetExternal => Asset?.IsExternal ?? false;

    /// <summary>
    /// The type of the referenced asset.
    /// </summary>
    public Type InstanceType => typeof(T);


    /// <summary>
    /// Creates an AssetReference referencing the specified <see cref="AssetManagement.Asset"/>.
    /// </summary>
    /// <param name="asset">The asset to reference.</param>
    private AssetReference(T? asset) => Asset = asset;


    /// <summary>
    /// Creates an AssetReference referencing the specified <see cref="AssetManagement.Asset"/>.
    /// </summary>
    /// <param name="asset">The asset to reference.</param>
    internal static AssetReference<T> Create(T asset) => new(asset);


    /// <summary>
    /// Discards the resolved content reference cache, and releases the asset.
    /// Equivalent to calling Dispose().
    /// </summary>
    public void Release() => Dispose();


    protected override void Dispose(bool manual)
    {
        if (IsDisposed)
            return;
        base.Dispose(manual);
        
        // NOTE: This could also be a warning, since no memory is actually leaked.
        if (!manual)
            throw new InvalidOperationException($"AssetReference<{typeof(T).Name}> for object '{Asset}' was not disposed of manually. Did you forget to call Release()?");
        
        if (this.TryRelease())
        {
            Asset?.Dispose();
            
            if (Asset?.IsExternal == true)
                AssetManager.NotifyUnloadAsset(Asset);
        }
        
        Asset = null;
        HasBeenReleased = true;
    }


    public override string ToString()
    {
        Type type = typeof(T);

        string state;
        if (HasBeenReleased)
            state = "Released";
        else if (Asset != null && Asset.IsDestroyed)
            state = "Disposed Prematurely";
        else
            state = "Alive";

        return $"[{state}] {type.Name}";
    }

    //public static implicit operator AssetReference<T>(T res) => new(res);
    //public static explicit operator T(AssetReference<T> res) => res.Asset!;


    public override int GetHashCode()
    {
        return Asset != null ? Asset.GetHashCode() : 0;
    }


#region Equality Checks

    public override bool Equals(object? obj)
    {
        if (obj is AssetReference<T> reference)
            return this == reference;
        return false;
    }


    /// <summary>
    /// Compares if the two AssetReferences reference the same asset.
    /// </summary>
    public bool Equals(AssetReference<T>? other) => this == other;


    /// <summary>
    /// Compares if the two AssetReferences reference the same asset.
    /// </summary>
    public static bool operator ==(AssetReference<T>? first, AssetReference<T>? second) => first?.Asset == second?.Asset;


    /// <summary>
    /// Compares if the two AssetReferences reference the same asset.
    /// </summary>
    public static bool operator !=(AssetReference<T>? first, AssetReference<T>? second) => !(first == second);

#endregion
}
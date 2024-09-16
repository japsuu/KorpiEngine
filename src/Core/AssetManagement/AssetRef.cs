using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

// Taken and modified from Prowl's AssetRef.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/AssetRef.cs,
// which is turn is taken and modified from Duality's ContentRef.cs
// https://github.com/AdamsLair/duality/blob/master/Source/Core/Duality/ContentRef.cs

/// <summary>
/// This lightweight struct references an <see cref="AssetManagement.Asset"/> in an abstract way.<br/>
/// It is tightly connected to the AssetDatabase, and takes care of keeping or making 
/// the referenced content available when needed.<br/>
/// Avoid storing actual Asset references permanently, instead use a AssetRef to it.<br/>
/// However, you may retrieve and store a direct Asset reference temporarily,
/// although this is only recommended at method-local scope.
/// </summary>
public struct AssetRef<T> : IEquatable<AssetRef<T>> where T : Asset
{
    private T? _reference;
    
    /// <summary>
    /// The ID of the asset in the asset database.<br/>
    /// None, if the asset is a runtime asset.
    /// </summary>
    private UUID _assetID = UUID.Empty;

    /// <summary>
    /// The subID of the asset in the asset database.<br/>
    /// 0 if the asset is the main asset of the external source.
    /// </summary>
    private ushort _subID = 0;

    /// <summary>
    /// The actual <see cref="AssetManagement.Asset"/>.
    /// If currently unavailable, it is loaded and then returned.
    /// Because of that, this Property is only null if the referenced Asset is missing, invalid, or
    /// this content reference has been explicitly set to null.
    /// Never returns destroyed Assets.
    /// </summary>
    public T? Asset
    {
        get
        {
            if (_reference == null || _reference.IsDestroyed)
                RetrieveReference();
            return _reference;
        }
        set
        {
            if (value == null)
            {
                _reference = null;
                _assetID = UUID.Empty;
                _subID = 0;
            }
            else
            {
                _reference = value;
                ExternalAssetInfo? info = value.ExternalInfo;
                _assetID = info?.AssetID ?? UUID.Empty;
                _subID = info?.SubID ?? 0;
            }
        }
    }

    /// <summary>
    /// Returns the current reference to the Asset that is stored locally.
    /// No attempt is made to load or reload the Asset if it is currently unavailable.
    /// </summary>
    public T? AssetWeak => _reference == null || _reference.IsDestroyed ? null : _reference;

    /// <summary>
    /// Returns whether this content reference has been explicitly set to null.
    /// </summary>
    public bool ReferencesNothing => _reference == null && _assetID == UUID.Empty;

    /// <summary>
    /// Returns whether this content reference is available in general.
    /// This may trigger loading it, if currently unavailable.
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            if (_reference != null && !_reference.IsDestroyed)
                return true;
            RetrieveReference();
            return _reference != null;
        }
    }

    /// <summary>
    /// Returns whether the referenced Asset is currently loaded.
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            if (_reference != null && !_reference.IsDestroyed)
                return true;
            
            return _assetID != UUID.Empty && AssetManager.Contains(_assetID);
        }
    }
    
    /// <summary>
    /// Whether the Asset is managed by the AssetManager.
    /// </summary>
    public bool IsExternal => _assetID != UUID.Empty;
    
    /// <summary>
    /// Whether the Asset is a sub-asset of another Asset.
    /// </summary>
    public bool IsSub => _subID != 0;

    /// <summary>
    /// Returns whether the Asset has been generated at runtime and cannot be retrieved via an AssetID.
    /// </summary>
    public bool IsRuntime => _reference != null && _assetID == UUID.Empty;

    public string Name
    {
        get
        {
            if (_reference != null)
                return _reference.IsDestroyed ? $"DESTROYED_{_reference.Name}" : _reference.Name;
            return "No Reference";
        }
    }

    public Type InstanceType => typeof(T);
    
    
    public static AssetRef<T> Empty => new();


    /// <summary>
    /// Creates a AssetRef pointing to the <see cref="AssetManagement.Asset"/> with the specified AssetID.
    /// </summary>
    /// <param name="id">The AssetID of the Asset to reference.</param>
    /// <param name="subID">The sub-ID of the Asset to reference. 0 for the main asset, 1+ for sub-assets.</param>
    public AssetRef(UUID id, ushort subID = 0)
    {
        _reference = null;
        _assetID = id;
        _subID = subID;
    }


    /// <summary>
    /// Creates a AssetRef pointing to the specified <see cref="AssetManagement.Asset"/>.
    /// </summary>
    /// <param name="res">The Asset to reference.</param>
    public AssetRef(T? res)
    {
        Asset = res;
    }


    /// <summary>
    /// Discards the resolved content reference cache.<br/>
    /// If the referenced Asset is not external, the reference CANNOT be restored.
    /// </summary>
    public void Release()
    {
        _reference = null;
    }


    private void RetrieveReference()
    {
        if (_assetID != UUID.Empty)
            Asset = AssetManager.LoadAsset<T>(_assetID, _subID);
        // Can potentially happen if the AssetRef was created with a runtime Asset that was later set to external
        else if (_reference != null && _reference.ExternalInfo != null && _reference.ExternalInfo.AssetID != UUID.Empty)
            Asset = AssetManager.LoadAsset<T>(_reference.ExternalInfo.AssetID, _reference.ExternalInfo.SubID);
        else
            Asset = null;
    }


    public override string ToString()
    {
        Type resType = typeof(T);

        char stateChar;
        if (IsRuntime)
            stateChar = 'R';
        else if (ReferencesNothing)
            stateChar = 'N';
        else if (IsLoaded)
            stateChar = 'L';
        else
            stateChar = '_';

        return $"[{stateChar}] {resType.Name}";
    }


    public override int GetHashCode()
    {
        if (_assetID != UUID.Empty)
            return _assetID.GetHashCode();
        if (_reference != null)
            return _reference.GetHashCode();
        return 0;
    }


    public override bool Equals(object? obj)
    {
        if (obj is AssetRef<T> @ref)
            return this == @ref;
        return base.Equals(obj);
    }


    public bool Equals(AssetRef<T> other) => this == other;

    public static implicit operator AssetRef<T>(T res) => new(res);
    public static explicit operator T(AssetRef<T> res) => res.Asset!;


    /// <summary>
    /// Compares two AssetRefs for equality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <remarks>
    /// This is a two-step comparison. First, their actual Assets references are compared.
    /// If they're both not null and equal, true is returned. Otherwise, their AssetID are compared to equality
    /// </remarks>
    public static bool operator ==(AssetRef<T> first, AssetRef<T> second)
    {
        // Completely identical
        if (first._reference == second._reference && first._assetID == second._assetID)
            return true;

        // Same instances
        if (first._reference != null && second._reference != null)
            return first._reference == second._reference;

        // Null checks
        if (first.ReferencesNothing)
            return second.ReferencesNothing;
        if (second.ReferencesNothing)
            return first.ReferencesNothing;

        // Path comparison
        UUID? firstPath = first._reference?.ExternalInfo?.AssetID ?? first._assetID;
        UUID? secondPath = second._reference?.ExternalInfo?.AssetID ?? second._assetID;
        return firstPath == secondPath;
    }


    /// <summary>
    /// Compares two AssetRefs for inequality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public static bool operator !=(AssetRef<T> first, AssetRef<T> second) => !(first == second);
}
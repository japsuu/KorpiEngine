using KorpiEngine.Tools.Serialization;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

// Taken and modified from Prowl's AssetRef.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/AssetRef.cs,
// which is turn is taken and modified from Duality's ContentRef.cs
// https://github.com/AdamsLair/duality/blob/master/Source/Core/Duality/ContentRef.cs

/// <summary>
/// This lightweight struct references an external <see cref="AssetInstance"/> in an abstract way.
/// It is tightly connected to the AssetDatabase, and takes care of keeping or making the referenced content available when needed.
/// Never store references to external assets permanently, instead use an <see cref="ExternalAssetRef{T}"/> to it.
/// However, you may retrieve and store a direct asset reference temporarily, although this is only recommended at method-local scope.
/// </summary>
public struct ExternalAssetRef<T> : ISerializable, IEquatable<ExternalAssetRef<T>> where T : AssetInstance
{
    private T? _assetReference;
    private UUID _assetID = UUID.Empty;
//#error Change to use AssetManager.Get on each property read.
    /// <summary>
    /// The referenced <see cref="AssetInstance"/>.
    /// If currently unavailable, it is loaded and then returned.
    /// Because of that, this property is only null if the referenced external asset is missing, invalid, or
    /// this content reference has been explicitly set to null. Never returns disposed Resources.
    /// </summary>
    public T? Asset
    {
        get
        {
            if (_assetReference == null || _assetReference.IsDestroyed)
                RetrieveReference();
            return _assetReference;
        }
        private set
        {
            _assetID = value?.ExternalAssetID ?? UUID.Empty;
            _assetReference = value;
        }
    }

    /// <summary>
    /// Returns the current reference to the Resource that is stored locally. No attempt is made to load or reload
    /// the Resource if it is currently unavailable.
    /// </summary>
    public T? ResWeak => _assetReference == null || _assetReference.IsDestroyed ? null : _assetReference;

    /// <summary>
    /// The path where to look for the Resource, if it is currently unavailable.
    /// </summary>
    public UUID AssetID
    {
        get => _assetID;
        set
        {
            _assetID = value;
            if (_assetReference != null && _assetReference.ExternalAssetID != value)
                _assetReference = null;
        }
    }

    /// <summary>
    /// Returns whether this content reference has been explicitly set to null.
    /// </summary>
    public bool IsExplicitNull => _assetReference == null && _assetID == UUID.Empty;

    /// <summary>
    /// Returns whether this content reference is available in general. This may trigger loading it, if currently unavailable.
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            if (_assetReference != null && !_assetReference.IsDestroyed)
                return true;
            RetrieveReference();
            return _assetReference != null;
        }
    }

    /// <summary>
    /// Returns whether the referenced Resource is currently loaded.
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            if (_assetReference != null && !_assetReference.IsDestroyed)
                return true;
            return AssetManager.Contains(_assetID);
        }
    }

    /// <summary>
    /// Returns whether the Resource has been generated at runtime and cannot be retrieved via content path.
    /// </summary>
    public bool IsRuntimeResource => _assetReference != null && _assetID == UUID.Empty;

    public string Name
    {
        get
        {
            if (_assetReference != null)
                return _assetReference.IsDestroyed ? "DESTROYED_" + _assetReference.Name : _assetReference.Name;
            return "No Instance";
        }
    }

    public Type InstanceType => typeof(T);


    /// <summary>
    /// Creates a AssetRef pointing to the <see cref="AssetInstance"/> at the specified id / using 
    /// the specified alias.
    /// </summary>
    /// <param name="id"></param>
    public ExternalAssetRef(UUID id)
    {
        _assetReference = null;
        _assetID = id;
    }


    /// <summary>
    /// Creates a AssetRef pointing to the specified <see cref="AssetInstance"/>.
    /// </summary>
    /// <param name="res">The Resource to reference.</param>
    public ExternalAssetRef(T? res)
    {
        _assetReference = res;
        _assetID = res?.ExternalAssetID ?? UUID.Empty;
    }


    public object? GetInstance() => Asset;


    public void SetInstance(object? obj)
    {
        if (obj is T res)
            Asset = res;
        else
            Asset = null;
    }


    /// <summary>
    /// Loads the associated content as if it was accessed now.
    /// You don't usually need to call this method. It is invoked implicitly by trying to 
    /// access the <see cref="ExternalAssetRef{T}"/>.
    /// </summary>
    public void EnsureLoaded()
    {
        if (_assetReference == null || _assetReference.IsDestroyed)
            RetrieveReference();
    }


    /// <summary>
    /// Discards the resolved content reference cache to allow garbage-collecting the Resource
    /// without losing its reference. Accessing it will result in reloading the Resource.
    /// </summary>
    public void Release()
    {
        _assetReference = null;
    }


    private void RetrieveReference()
    {
        if (_assetID != UUID.Empty)
            _assetReference = AssetManager.LoadAsset<T>(_assetID);
        else if (_assetReference != null && _assetReference.ExternalAssetID != UUID.Empty)
            _assetReference = AssetManager.LoadAsset<T>(_assetReference.ExternalAssetID);
        else
            _assetReference = null;
    }


    public override string ToString()
    {
        Type resType = typeof(T);

        char stateChar;
        if (IsRuntimeResource)
            stateChar = 'R';
        else if (IsExplicitNull)
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
        if (_assetReference != null)
            return _assetReference.GetHashCode();
        return 0;
    }


    public override bool Equals(object? obj)
    {
        if (obj is ExternalAssetRef<T> @ref)
            return this == @ref;
        return base.Equals(obj);
    }


    public bool Equals(ExternalAssetRef<T> other) => this == other;

    public static implicit operator ExternalAssetRef<T>(T res) => new(res);

    public static explicit operator T(ExternalAssetRef<T> res) => res.Asset!;


    /// <summary>
    /// Compares two AssetRefs for equality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <remarks>
    /// This is a two-step comparison. First, their actual Resources references are compared.
    /// If they're both not null and equal, true is returned. Otherwise, their AssetID are compared to equality
    /// </remarks>
    public static bool operator ==(ExternalAssetRef<T> first, ExternalAssetRef<T> second)
    {
        // Completely identical
        if (first._assetReference == second._assetReference && first._assetID == second._assetID)
            return true;

        // Same instances
        if (first._assetReference != null && second._assetReference != null)
            return first._assetReference == second._assetReference;

        // Null checks
        if (first.IsExplicitNull)
            return second.IsExplicitNull;
        if (second.IsExplicitNull)
            return first.IsExplicitNull;

        // Path comparison
        UUID? firstPath = first._assetReference?.ExternalAssetID ?? first._assetID;
        UUID? secondPath = second._assetReference?.ExternalAssetID ?? second._assetID;
        return firstPath == secondPath;
    }


    /// <summary>
    /// Compares two AssetRefs for inequality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public static bool operator !=(ExternalAssetRef<T> first, ExternalAssetRef<T> second) => !(first == second);


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();
        compoundTag.Add("AssetID", new SerializedProperty(_assetID.ToString()));
        if (_assetID != UUID.Empty)
            ctx.AddDependency(_assetID);
        if (IsRuntimeResource)
            compoundTag.Add("Instance", Serializer.Serialize(_assetReference, ctx));
        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        _assetID = UUID.Parse(value["AssetID"].StringValue);
        if (_assetID == UUID.Empty && value.TryGet("Instance", out SerializedProperty? tag))
            _assetReference = Serializer.Deserialize<T?>(tag!, ctx);
    }
}
using KorpiEngine.Core.API.AssetManagement;
using KorpiEngine.Core.Internal.Serialization;

namespace KorpiEngine.Core.Internal.AssetManagement;

// Taken and modified from Prowl's AssetRef.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/AssetRef.cs,
// which is turn is taken and modified from Duality's ContentRef.cs
// https://github.com/AdamsLair/duality/blob/master/Source/Core/Duality/ContentRef.cs

/// <summary>
/// This lightweight struct references an <see cref="Resource"/> in an abstract way.
/// It is tightly connected to the AssetDatabase, and takes care of keeping or making 
/// the referenced content available when needed. Never store actual Resource references permanently,
/// instead use a ResourceRef to it. However, you may retrieve and store a direct Resource reference
/// temporarily, although this is only recommended at method-local scope.
/// </summary>
public struct ResourceRef<T> : ISerializable, IEquatable<ResourceRef<T>> where T : Resource
{
    private T? _instance;
    private Guid _assetID = Guid.Empty;

    /// <summary>
    /// The actual <see cref="Resource"/>.
    /// If currently unavailable, it is loaded and then returned.
    /// Because of that, this Property is only null if the referenced Resource is missing, invalid, or
    /// this content reference has been explicitly set to null. Never returns disposed Resources.
    /// </summary>
    public T? Res
    {
        get
        {
            if (_instance == null || _instance.IsDestroyed)
                RetrieveInstance();
            return _instance;
        }
        private set
        {
            _assetID = value?.AssetID ?? Guid.Empty;
            _instance = value;
        }
    }

    /// <summary>
    /// Returns the current reference to the Resource that is stored locally. No attempt is made to load or reload
    /// the Resource if it is currently unavailable.
    /// </summary>
    public T? ResWeak => _instance == null || _instance.IsDestroyed ? null : _instance;

    /// <summary>
    /// The path where to look for the Resource, if it is currently unavailable.
    /// </summary>
    public Guid AssetID
    {
        get => _assetID;
        set
        {
            _assetID = value;
            if (_instance != null && _instance.AssetID != value)
                _instance = null;
        }
    }

    /// <summary>
    /// Returns whether this content reference has been explicitly set to null.
    /// </summary>
    public bool IsExplicitNull => _instance == null && _assetID == Guid.Empty;

    /// <summary>
    /// Returns whether this content reference is available in general. This may trigger loading it, if currently unavailable.
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            if (_instance != null && !_instance.IsDestroyed)
                return true;
            RetrieveInstance();
            return _instance != null;
        }
    }

    /// <summary>
    /// Returns whether the referenced Resource is currently loaded.
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            if (_instance != null && !_instance.IsDestroyed)
                return true;
            return AssetDatabase.Contains(_assetID);
        }
    }

    /// <summary>
    /// Returns whether the Resource has been generated at runtime and cannot be retrieved via content path.
    /// </summary>
    public bool IsRuntimeResource => _instance != null && _assetID == Guid.Empty;

    public string Name
    {
        get
        {
            if (_instance != null)
                return _instance.IsDestroyed ? "DESTROYED_" + _instance.Name : _instance.Name;
            return "No Instance";
        }
    }

    public Type InstanceType => typeof(T);


    /// <summary>
    /// Creates a AssetRef pointing to the <see cref="Resource"/> at the specified id / using 
    /// the specified alias.
    /// </summary>
    /// <param name="id"></param>
    public ResourceRef(Guid id)
    {
        _instance = null;
        _assetID = id;
    }


    /// <summary>
    /// Creates a AssetRef pointing to the specified <see cref="Resource"/>.
    /// </summary>
    /// <param name="res">The Resource to reference.</param>
    public ResourceRef(T? res)
    {
        _instance = res;
        _assetID = res?.AssetID ?? Guid.Empty;
    }


    public object? GetInstance() => Res;


    public void SetInstance(object? obj)
    {
        if (obj is T res)
            Res = res;
        else
            Res = null;
    }


    /// <summary>
    /// Loads the associated content as if it was accessed now.
    /// You don't usually need to call this method. It is invoked implicitly by trying to 
    /// access the <see cref="ResourceRef{T}"/>.
    /// </summary>
    public void EnsureLoaded()
    {
        if (_instance == null || _instance.IsDestroyed)
            RetrieveInstance();
    }


    /// <summary>
    /// Discards the resolved content reference cache to allow garbage-collecting the Resource
    /// without losing its reference. Accessing it will result in reloading the Resource.
    /// </summary>
    public void Detach()
    {
        _instance = null;
    }


    private void RetrieveInstance()
    {
        if (_assetID != Guid.Empty)
            _instance = AssetDatabase.LoadAsset<T>(_assetID);
        else if (_instance != null && _instance.AssetID != Guid.Empty)
            _instance = AssetDatabase.LoadAsset<T>(_instance.AssetID);
        else
            _instance = null;
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
        if (_assetID != Guid.Empty)
            return _assetID.GetHashCode();
        if (_instance != null)
            return _instance.GetHashCode();
        return 0;
    }


    public override bool Equals(object? obj)
    {
        if (obj is ResourceRef<T> @ref)
            return this == @ref;
        return base.Equals(obj);
    }


    public bool Equals(ResourceRef<T> other) => this == other;

    public static implicit operator ResourceRef<T>(T res) => new(res);

    public static explicit operator T(ResourceRef<T> res) => res.Res!;


    /// <summary>
    /// Compares two AssetRefs for equality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <remarks>
    /// This is a two-step comparison. First, their actual Resources references are compared.
    /// If they're both not null and equal, true is returned. Otherwise, their AssetID are compared to equality
    /// </remarks>
    public static bool operator ==(ResourceRef<T> first, ResourceRef<T> second)
    {
        // Completely identical
        if (first._instance == second._instance && first._assetID == second._assetID)
            return true;

        // Same instances
        if (first._instance != null && second._instance != null)
            return first._instance == second._instance;

        // Null checks
        if (first.IsExplicitNull)
            return second.IsExplicitNull;
        if (second.IsExplicitNull)
            return first.IsExplicitNull;

        // Path comparison
        Guid? firstPath = first._instance?.AssetID ?? first._assetID;
        Guid? secondPath = second._instance?.AssetID ?? second._assetID;
        return firstPath == secondPath;
    }


    /// <summary>
    /// Compares two AssetRefs for inequality.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public static bool operator !=(ResourceRef<T> first, ResourceRef<T> second) => !(first == second);


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();
        compoundTag.Add("AssetID", new SerializedProperty(_assetID.ToString()));
        if (_assetID != Guid.Empty)
            ctx.AddDependency(_assetID);
        if (IsRuntimeResource)
            compoundTag.Add("Instance", Serializer.Serialize(_instance, ctx));
        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        _assetID = Guid.Parse(value["AssetID"].StringValue);
        if (_assetID == Guid.Empty && value.TryGet("Instance", out SerializedProperty? tag))
            _instance = Serializer.Deserialize<T?>(tag!, ctx);
    }
}
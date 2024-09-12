using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public abstract class Asset : SafeDisposable
{
    private static class AssetID
    {
        /// <summary>
        /// The next ID to be assigned to a resource.
        /// Starts at 1, because the ObjectID buffer (in G-Buffer) uses 0 as a "null" value.
        /// </summary>
        private static int nextID = 1;
    
    
        public static int Generate()
        {
            int id = Interlocked.Increment(ref nextID);
            Debug.Assert(id != int.MaxValue, "AssetInstanceID overflow!");
            return id;
        }
    }
    
    private static readonly Stack<Asset> DisposeDeferredAssets = new();
    private static readonly Dictionary<int, WeakReference<Asset>> AllAssets = new();

    /// <summary>
    /// Unique identifier for this asset.
    /// </summary>
    public readonly int InstanceID;
    
    /// <summary>
    /// The name of the asset.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The UUID of the external asset from which this asset was loaded.
    /// Empty if the asset is not external.
    /// </summary>
    public UUID ExternalAssetID { get; internal set; } = UUID.Empty;
    
    /// <summary>
    /// Whether the asset is external (loaded from a file).
    /// If this property is true, the asset is managed by the AssetManager and should not be disposed of manually.
    /// </summary>
    public bool IsExternal { get; internal set; }

    /// <summary>
    /// Whether the asset has been destroyed (disposed).
    /// </summary>
    public bool IsDestroyed => IsDisposed;

    /// <summary>
    /// The number of references to this asset.
    /// </summary>
    public int ReferenceCount
    {
        get => _referenceCount;
        set
        {
            if (value < 0)
                throw new InvalidOperationException("Reference count cannot be negative.");
            _referenceCount = value;
        }
    }

    private int _referenceCount;
    private bool _isWaitingDisposal;


    #region Creation, Destruction, and Disposal

    protected Asset(string? name = null)
    {
        InstanceID = AssetID.Generate();
        AllAssets.Add(InstanceID, new WeakReference<Asset>(this));

        Name = name ?? $"New {GetType().Name} Asset";
    }


    /// <summary>
    /// Queues this asset for disposal.
    /// The disposing will be deferred until the end of the frame.
    /// </summary>
    /// <exception cref="AssetReleaseException">Thrown if the asset is already disposed.</exception>
    public void DisposeDeferred()
    {
        if (IsDisposed || _isWaitingDisposal)
            return;
        
        if (ReferenceCount > 0)
            throw new AssetReleaseException("Cannot dispose of an asset with active references.");
        
        _isWaitingDisposal = true;
        DisposeDeferredAssets.Push(this);
    }


    protected sealed override void Dispose(bool manual)
    {
        if (IsDisposed || _isWaitingDisposal)
            return;
        
        if (ReferenceCount > 0)
            throw new AssetReleaseException("Cannot dispose of an asset with active references.");

        base.Dispose(manual);

        OnDispose(manual);
        AllAssets.Remove(InstanceID);
    }


    internal static void ProcessDisposeQueue()
    {
        while (DisposeDeferredAssets.TryPop(out Asset? obj))
        {
            if (obj.IsDisposed)
                continue;

            obj._isWaitingDisposal = false;
            obj.Dispose();
        }
    }

    #endregion


    #region Public Static Methods

    public static T? FindObjectOfType<T>() where T : Asset
    {
        foreach (WeakReference<Asset> obj in AllAssets.Values)
            if (obj.TryGetTarget(out Asset? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : Asset
    {
        List<T> objects = [];
        foreach (WeakReference<Asset> obj in AllAssets.Values)
            if (obj.TryGetTarget(out Asset? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : Asset
    {
        if (!AllAssets.TryGetValue(id, out WeakReference<Asset>? obj))
            return null;

        if (!obj.TryGetTarget(out Asset? target) || target is not T t)
            return null;

        return t;
    }

    #endregion


    /// <param name="manual">True, if the call is performed explicitly by calling <see cref="Dispose"/> or <see cref="DisposeDeferred"/>.<br/>
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void OnDispose(bool manual) { }
    public override string ToString() => $"{Name} ({GetType().FullName}) [{InstanceID}]";
}
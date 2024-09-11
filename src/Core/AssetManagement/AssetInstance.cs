using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public abstract class AssetInstance
{
    private static class AssetInstanceID
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
    
    private static readonly Stack<AssetInstance> DisposeDeferredAssets = new();
    private static readonly Dictionary<int, WeakReference<AssetInstance>> AllAssets = new();

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
    /// Whether the asset has been destroyed.
    /// </summary>
    public bool IsDestroyed { get; private set; }
    private bool _isWaitingRelease;


    #region Creation, Destruction, and Disposal

    protected AssetInstance(string? name = null)
    {
        InstanceID = AssetInstanceID.Generate();
        AllAssets.Add(InstanceID, new WeakReference<AssetInstance>(this));

        Name = name ?? $"New {GetType().Name} Asset";
    }
    
    
    ~AssetInstance()
    {
        Dispose(false);
    }


    internal void Dispose()
    {
        Dispose(true);
        // Take this object off the finalization queue to prevent the destructor from being called.
        GC.SuppressFinalize(this);
    }


    private void Dispose(bool manual)
    {
        if (IsDestroyed)
            return;
        IsDestroyed = true;
        
        if (IsExternal)
        {
            // This should never fail because of the reference counting system.
            Debug.Assert(!manual, $"External asset {Name} ({GetType().FullName}) was released by the GC. This is a memory leak! Did you forget to call Release() or AssetManager.UnloadAsset()?");
            throw new InvalidOperationException($"{Name} ({GetType().FullName}) is an external asset and cannot be released manually.");
        }

        OnDispose(manual);
        AllAssets.Remove(InstanceID);
    }


    /// <summary>
    /// Queues this asset for disposal.
    /// The asset will be disposed of at the end of the frame.
    /// </summary>
    /// <exception cref="AssetReleasedException">Thrown if the asset is already released.</exception>
    public void Release()
    {
        if (IsDestroyed)
            throw new AssetReleasedException($"{Name} ({GetType().FullName}) has already been released.");
        
        _isWaitingRelease = true;
        DisposeDeferredAssets.Push(this);
    }


    /// <summary>
    /// Disposes of this asset, releasing it immediately.
    /// Should only be called if the asset is not external.
    /// </summary>
    /// <exception cref="AssetReleasedException">Thrown if the asset is already released.</exception>
    public void ReleaseImmediate()
    {
        if (IsDestroyed)
            throw new AssetReleasedException($"{Name} ({GetType().FullName}) has already been released.");
        
        HandleRelease();
    }
    
    
    private void HandleRelease()
    {
        _isWaitingRelease = false;

        if (IsExternal)
            // Decreases the reference count of the external asset, and call Dispose it if it reaches 0.
            AssetManager.UnloadAsset(ExternalAssetID);
        else
            Dispose();
    }


    internal static void ProcessReleaseQueue()
    {
        while (DisposeDeferredAssets.TryPop(out AssetInstance? obj))
        {
            if (obj.IsDestroyed)
                continue;

            obj.HandleRelease();
        }
    }

    #endregion


    #region Public Static Methods

    public static T? FindObjectOfType<T>() where T : AssetInstance
    {
        foreach (WeakReference<AssetInstance> obj in AllAssets.Values)
            if (obj.TryGetTarget(out AssetInstance? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : AssetInstance
    {
        List<T> objects = [];
        foreach (WeakReference<AssetInstance> obj in AllAssets.Values)
            if (obj.TryGetTarget(out AssetInstance? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : AssetInstance
    {
        if (!AllAssets.TryGetValue(id, out WeakReference<AssetInstance>? obj))
            return null;

        if (!obj.TryGetTarget(out AssetInstance? target) || target is not T t)
            return null;

        return t;
    }

    #endregion


    /// <param name="manual">True, if the call is performed explicitly by calling <see cref="Release"/> or <see cref="ReleaseImmediate"/>.<br/>
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void OnDispose(bool manual) { }
    public override string ToString() => Name;
}
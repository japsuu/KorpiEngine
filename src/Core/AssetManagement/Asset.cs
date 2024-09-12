using System.Text;
using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Base class for "resource types", serving primarily as data containers.
/// Assets can be manually disposed, but are also automatically collected by the GC.
/// </summary>
public abstract class Asset : SafeDisposable
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
    
    private static readonly Stack<Asset> DisposalDelayedResources = new();
    private static readonly Dictionary<int, WeakReference<Asset>> AllResources = new();

    /// <summary>
    /// Unique identifier for this resource.
    /// </summary>
    public readonly int InstanceID;
    
    /// <summary>
    /// The name of the asset.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The ID of the asset in the asset database.
    /// None, if the asset is a runtime asset.
    /// </summary>
    public UUID ExternalAssetID { get; internal set; } = UUID.Empty;
    
    /// <summary>
    /// Whether the asset has been loaded from an external source.
    /// If true, <see cref="ExternalAssetID"/> will be set.
    /// </summary>
    public bool IsExternal { get; internal set; }
    
    /// <summary>
    /// Whether the underlying object has been destroyed (disposed or waiting for disposal).
    /// </summary>
    public bool IsDestroyed => IsDisposed || IsWaitingDisposal;
    public bool IsWaitingDisposal { get; private set; }


    #region Creation, Destruction, and Disposal

    protected Asset(string? name = "New Resource")
    {
        InstanceID = AssetInstanceID.Generate();
        AllResources.Add(InstanceID, new WeakReference<Asset>(this));

        Name = name ?? $"New {GetType().Name}";
    }
    
    
    ~Asset()
    {
        Dispose(false);
    }


    protected sealed override void Dispose(bool manual)
    {
        if (IsDisposed)
            return;
        base.Dispose(manual);
        
        OnDispose(manual);
        AllResources.Remove(InstanceID);
    }


    /// <summary>
    /// Queues this resource for disposal.
    /// The resource will be destroyed at the end of the frame.
    /// </summary>
    /// <exception cref="AssetDestroyedException">Thrown if the resource is already destroyed.</exception>
    public void Destroy()
    {
        if (IsDestroyed)
            throw new AssetDestroyedException($"{Name} is already destroyed.");
        
        IsWaitingDisposal = true;
        DisposalDelayedResources.Push(this);
    }


    /// <summary>
    /// Calls <see cref="Dispose"/> on this resource, destroying it immediately.
    /// </summary>
    /// <exception cref="AssetDestroyedException">Thrown if the resource is already destroyed.</exception>
    public void DestroyImmediate()
    {
        if (IsDestroyed)
            throw new AssetDestroyedException($"{Name} is already destroyed.");
        
        Dispose();
    }


    internal static void HandleDestroyed()
    {
        while (DisposalDelayedResources.TryPop(out Asset? obj))
        {
            if (obj.IsDisposed)
                continue;

            obj.Dispose();
        }
    }

    #endregion


    #region Public Static Methods

    public static T? FindObjectOfType<T>() where T : Asset
    {
        foreach (WeakReference<Asset> obj in AllResources.Values)
            if (obj.TryGetTarget(out Asset? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : Asset
    {
        List<T> objects = [];
        foreach (WeakReference<Asset> obj in AllResources.Values)
            if (obj.TryGetTarget(out Asset? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : Asset
    {
        if (!AllResources.TryGetValue(id, out WeakReference<Asset>? obj))
            return null;

        if (!obj.TryGetTarget(out Asset? target) || target is not T t)
            return null;

        return t;
    }

    #endregion


    /// <summary>
    /// Releases all owned resources.
    /// Guaranteed to be called only once.<br/><br/>
    /// 
    /// Example implementation:
    /// <code>
    /// protected override void OnDispose(bool manual)
    /// {
    ///     if (manual)
    ///     {
    ///         // Dispose managed resources
    ///     }
    ///     
    ///     // Dispose unmanaged resources
    /// }
    /// </code>
    /// </summary>
    /// <param name="manual">True, if the call is performed explicitly by calling <see cref="Dispose"/>.
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void OnDispose(bool manual) { }
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append(" (");
        sb.Append(GetType().Name);
        sb.Append(") [");
        sb.Append(InstanceID);
        if (IsExternal)
        {
            sb.Append(" - ");
            sb.Append(ExternalAssetID);
        }
        sb.Append(']');
        
        return sb.ToString();
    }
}
using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public abstract class AssetInstance : SafeDisposable
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
    
    private static readonly Stack<AssetInstance> ReleaseDeferredResources = new();
    private static readonly Dictionary<int, WeakReference<AssetInstance>> AllResources = new();

    /// <summary>
    /// Unique identifier for this resource.
    /// </summary>
    public readonly int InstanceID;
    public string Name { get; set; }
    
    // Asset path if we have one
    public UUID AssetID { get; internal set; } = UUID.Empty;
    
    /// <summary>
    /// Whether the object has been released (disposed or waiting for disposal).
    /// </summary>
    public bool IsReleased => IsDisposed || IsWaitingDisposal;
    public bool IsWaitingDisposal { get; private set; }


    #region Creation, Destruction, and Disposal

    protected AssetInstance(string? name = "New Resource")
    {
        InstanceID = AssetInstanceID.Generate();
        AllResources.Add(InstanceID, new WeakReference<AssetInstance>(this));

        Name = name ?? $"New {GetType().Name}";
    }
    
    
    ~AssetInstance()
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
    /// The resource will be released at the end of the frame.
    /// </summary>
    /// <exception cref="AssetReleasedException">Thrown if the resource is already released.</exception>
    public void Release()
    {
        if (IsReleased)
            throw new AssetReleasedException($"{Name} ({GetType().FullName}) has already been released.");
        
        IsWaitingDisposal = true;
        ReleaseDeferredResources.Push(this);
    }


    /// <summary>
    /// Calls <see cref="Dispose"/> on this resource, releasing it immediately.
    /// </summary>
    /// <exception cref="AssetReleasedException">Thrown if the resource is already released.</exception>
    public void ReleaseImmediate()
    {
        if (IsReleased)
            throw new AssetReleasedException($"{Name} ({GetType().FullName}) has already been released.");
        
        Dispose();
    }


    internal static void ProcessReleaseQueue()
    {
        while (ReleaseDeferredResources.TryPop(out AssetInstance? obj))
        {
            if (obj.IsDisposed)
                continue;

            obj.Dispose();
        }
    }

    #endregion


    #region Public Static Methods

    public static T? FindObjectOfType<T>() where T : AssetInstance
    {
        foreach (WeakReference<AssetInstance> obj in AllResources.Values)
            if (obj.TryGetTarget(out AssetInstance? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : AssetInstance
    {
        List<T> objects = [];
        foreach (WeakReference<AssetInstance> obj in AllResources.Values)
            if (obj.TryGetTarget(out AssetInstance? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : AssetInstance
    {
        if (!AllResources.TryGetValue(id, out WeakReference<AssetInstance>? obj))
            return null;

        if (!obj.TryGetTarget(out AssetInstance? target) || target is not T t)
            return null;

        return t;
    }

    #endregion


    /// <param name="manual">True, if the call is performed explicitly by calling <see cref="Dispose"/>.
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread and the result of a resource leak.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void OnDispose(bool manual) { }
    public override string ToString() => Name;
}
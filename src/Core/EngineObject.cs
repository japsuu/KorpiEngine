using System.Text;
using KorpiEngine.AssetManagement;
using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine;

/// <summary>
/// Base class for most engine types.<br/>
/// Please avoid calling <see cref="SafeDisposable.Dispose"/> manually, use <see cref="Destroy"/> instead.
/// </summary>
public class EngineObject : SafeDisposable
{
    private static class ObjectInstanceID
    {
        /// <summary>
        /// The next ID to be assigned to an object.
        /// Starts at 1, because the ObjectID buffer (in G-Buffer) uses 0 as a "null" value.
        /// </summary>
        private static int nextID = 1;
    
    
        public static int Generate()
        {
            Debug.Assert(nextID != int.MaxValue, "InstanceID overflow!");
            int id = Interlocked.Increment(ref nextID);
            return id;
        }
    }
    
    private static readonly Dictionary<int, WeakReference<EngineObject>> AllResources = new();
    private static readonly Stack<EngineObject> DisposalDelayedResources = new();

    /// <summary>
    /// Unique identifier for this resource.
    /// </summary>
    public readonly int InstanceID;
    
    /// <summary>
    /// The name of the object.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Whether the underlying object has been destroyed (disposed or waiting for disposal).
    /// </summary>
    public bool IsDestroyed => IsDisposed || IsWaitingDisposal;
    public bool IsWaitingDisposal { get; private set; }


#region Creation, Destruction, and Disposal

    protected EngineObject(string name)
    {
        InstanceID = ObjectInstanceID.Generate();
        AllResources.Add(InstanceID, new WeakReference<EngineObject>(this));

        Name = string.IsNullOrEmpty(name) ? $"New {GetType().Name} Object" : name;
    }
    
    
    ~EngineObject()
    {
        Dispose(false);
    }


    /// <summary>
    /// Queues this object for disposal.
    /// The resource will be disposed of at the end of the frame.
    /// </summary>
    /// <exception cref="ObjectDestroyedException">Thrown if the object is already destroyed.</exception>
    public void Destroy()
    {
        if (IsDestroyed)
            throw new ObjectDestroyedException($"{Name} is already destroyed.");
        
        IsWaitingDisposal = true;
        DisposalDelayedResources.Push(this);
    }


    /// <summary>
    /// Calls <see cref="Dispose"/> on this object, destroying it immediately.
    /// </summary>
    /// <exception cref="ObjectDestroyedException">Thrown if the object is already destroyed.</exception>
    public void DestroyImmediate()
    {
        if (IsDestroyed)
            throw new ObjectDestroyedException($"{Name} is already destroyed.");
        
        Dispose();
    }


    protected sealed override void Dispose(bool manual)
    {
        if (IsDisposed)
            return;
        base.Dispose(manual);
        
        OnDispose(manual);
        AllResources.Remove(InstanceID);
    }


    internal static void ProcessDisposeQueue()
    {
        while (DisposalDelayedResources.TryPop(out EngineObject? obj))
        {
            if (obj.IsDisposed)
                continue;

            obj.Dispose();
        }
    }

#endregion


#region Public Static "FindObject" Methods

    public static T? FindObjectOfType<T>() where T : EngineObject
    {
        foreach (WeakReference<EngineObject> obj in AllResources.Values)
            if (obj.TryGetTarget(out EngineObject? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : EngineObject
    {
        List<T> objects = [];
        foreach (WeakReference<EngineObject> obj in AllResources.Values)
            if (obj.TryGetTarget(out EngineObject? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : EngineObject
    {
        if (!AllResources.TryGetValue(id, out WeakReference<EngineObject>? obj))
            return null;

        if (!obj.TryGetTarget(out EngineObject? target) || target is not T t)
            return null;

        return t;
    }

#endregion


#region Protected Virtual Methods

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

#endregion
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append(" (");
        sb.Append(GetType().Name);
        sb.Append(") [InstanceID: ");
        sb.Append(InstanceID);
        sb.Append(']');
        
        return sb.ToString();
    }
}
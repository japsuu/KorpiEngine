﻿using KorpiEngine.EntityModel.IDs;
using KorpiEngine.Exceptions;

namespace KorpiEngine;

public abstract class Resource : SafeDisposable
{
    private static readonly Stack<Resource> DestroyedResources = new();
    private static readonly Dictionary<int, WeakReference<Resource>> AllResources = new();

    /// <summary>
    /// Unique identifier for this resource.
    /// </summary>
    public readonly int InstanceID;
    public string Name { get; set; }
    
    // Asset path if we have one
    public Guid AssetID { get; internal set; } = Guid.Empty;
    
    /// <summary>
    /// Whether the underlying object has been destroyed (disposed or waiting for disposal).
    /// </summary>
    public bool IsDestroyed => IsDisposed || IsWaitingDisposal;
    public bool IsWaitingDisposal { get; private set; }


    #region Creation, Destruction, and Disposal

    protected Resource(string? name = "New Resource")
    {
        InstanceID = ResourceID.Generate();
        AllResources.Add(InstanceID, new WeakReference<Resource>(this));

        Name = name ?? $"New {GetType().Name}";
    }
    
    
    ~Resource()
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
    /// <exception cref="ResourceDestroyedException">Thrown if the resource is already destroyed.</exception>
    public void Destroy()
    {
        if (IsDestroyed)
            throw new ResourceDestroyedException($"{Name} is already destroyed.");
        
        IsWaitingDisposal = true;
        DestroyedResources.Push(this);
    }


    /// <summary>
    /// Calls <see cref="Dispose"/> on this resource, destroying it immediately.
    /// </summary>
    /// <exception cref="ResourceDestroyedException">Thrown if the resource is already destroyed.</exception>
    public void DestroyImmediate()
    {
        if (IsDestroyed)
            throw new ResourceDestroyedException($"{Name} is already destroyed.");
        
        Dispose();
    }


    internal static void HandleDestroyed()
    {
        while (DestroyedResources.TryPop(out Resource? obj))
        {
            if (!obj.IsDestroyed)
                continue;

            obj.Dispose();
        }
    }

    #endregion


    #region Public Static Methods

    public static T? FindObjectOfType<T>() where T : Resource
    {
        foreach (WeakReference<Resource> obj in AllResources.Values)
            if (obj.TryGetTarget(out Resource? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : Resource
    {
        List<T> objects = [];
        foreach (WeakReference<Resource> obj in AllResources.Values)
            if (obj.TryGetTarget(out Resource? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : Resource
    {
        if (!AllResources.TryGetValue(id, out WeakReference<Resource>? obj))
            return null;

        if (!obj.TryGetTarget(out Resource? target) || target is not T t)
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
using KorpiEngine.Core.Internal.Serialization;

namespace KorpiEngine.Core;

public abstract class EngineObject : IDisposable
{
    private static readonly Stack<EngineObject> DestroyedObjects = new();
    private static readonly Dictionary<int, WeakReference<EngineObject>> AllObjects = new();
    private static int nextID;

    public readonly int InstanceID;
    public string Name;
    
    // Asset path if we have one
    public Guid AssetID = Guid.Empty;
    
    /// <summary>
    /// Whether the underlying object has been destroyed.
    /// </summary>
    public bool IsDestroyed { get; private set; }


    public EngineObject() : this(null)
    {
    }


    public EngineObject(string? name = "New Object")
    {
        InstanceID = nextID++;
        AllObjects.Add(InstanceID, new WeakReference<EngineObject>(this));

        if (name == null)
            Name = "New " + GetType().Name;
        else
            Name = name;
    }


    public static T? FindObjectOfType<T>() where T : EngineObject
    {
        foreach (WeakReference<EngineObject> obj in AllObjects.Values)
            if (obj.TryGetTarget(out EngineObject? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : EngineObject
    {
        List<T> objects = new();
        foreach (WeakReference<EngineObject> obj in AllObjects.Values)
            if (obj.TryGetTarget(out EngineObject? target) && target is T t)
                objects.Add(t);
        return objects.ToArray();
    }


    public static T? FindObjectByID<T>(int id) where T : EngineObject
    {
        if (!AllObjects.TryGetValue(id, out WeakReference<EngineObject>? obj))
            return null;

        if (!obj.TryGetTarget(out EngineObject? target) || target is not T t)
            return null;

        return t;
    }


    public void Destroy() => Destroy(this);

    public void DestroyImmediate() => DestroyImmediate(this);


    public static void Destroy(EngineObject obj)
    {
        if (obj.IsDestroyed)
            throw new Exception(obj.Name + " is already destroyed.");
        obj.IsDestroyed = true;
        DestroyedObjects.Push(obj);
    }


    public static void DestroyImmediate(EngineObject obj)
    {
        if (obj.IsDestroyed)
            throw new Exception(obj.Name + " is already destroyed.");
        obj.IsDestroyed = true;
        obj.Dispose();
    }


    public static void HandleDestroyed()
    {
        while (DestroyedObjects.TryPop(out EngineObject? obj))
        {
            if (!obj.IsDestroyed)
                continue;

            obj.Dispose();
        }
    }


    public static EngineObject Instantiate(EngineObject obj, bool keepAssetID = false)
    {
#warning TODO: Implement custom serialization for EngineObject
        if (obj.IsDestroyed)
            throw new Exception(obj.Name + " has been destroyed.");

        // Serialize and deserialize to get a new object
        SerializedProperty serialized = Serializer.Serialize(obj);

        // dont need to assign ID or add it to objects list the constructor will do that automatically
        EngineObject? newObj = Serializer.Deserialize<EngineObject>(serialized);

        // Some objects might have a readonly name (like components) in that case it should remain the same, so if name is different set it
        newObj!.Name = obj.Name;

        // Need to make sure to set GUID to empty so the engine knows this isn't the original Asset file
        if (!keepAssetID)
            newObj.AssetID = Guid.Empty;
        return newObj;
    }
    


    /// <summary>
    /// Force the object to dispose immediately
    /// You are advised to not use this! Use Destroy() Instead.
    /// </summary>
    public void Dispose()
    {
        IsDestroyed = true;
        GC.SuppressFinalize(this);
        OnDispose();
        AllObjects.Remove(InstanceID);
    }


    protected virtual void OnDispose() { }


    public override string ToString() => Name;
}
namespace KorpiEngine.Core;

public abstract class Resource : IDisposable
{
    private static readonly Stack<Resource> DestroyedResources = new();
    private static readonly Dictionary<int, WeakReference<Resource>> AllResources = new();
    private static int nextID;

    public readonly int InstanceID;
    public string Name;
    
    // Asset path if we have one
    public Guid AssetID { get; internal set; } = Guid.Empty;
    
    /// <summary>
    /// Whether the underlying object has been destroyed, or is waiting for destruction.
    /// </summary>
    public bool IsDestroyed { get; private set; }


    protected Resource(string? name = "New Resource")
    {
        InstanceID = nextID++;
        AllResources.Add(InstanceID, new WeakReference<Resource>(this));

        if (name == null)
            Name = "New " + GetType().Name;
        else
            Name = name;
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
        AllResources.Remove(InstanceID);
    }


    public void Destroy() => Destroy(this);

    public void DestroyImmediate() => DestroyImmediate(this);


    private static void Destroy(Resource obj)
    {
        if (obj.IsDestroyed)
            throw new Exception(obj.Name + " is already destroyed.");
        obj.IsDestroyed = true;
        DestroyedResources.Push(obj);
    }


    private static void DestroyImmediate(Resource obj)
    {
        if (obj.IsDestroyed)
            throw new Exception(obj.Name + " is already destroyed.");
        obj.IsDestroyed = true;
        obj.Dispose();
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


    protected virtual void OnDispose() { }
    public override string ToString() => Name;


    /*public static T? FindObjectOfType<T>() where T : Resource
    {
        foreach (WeakReference<Resource> obj in AllResources.Values)
            if (obj.TryGetTarget(out Resource? target) && target is T t)
                return t;
        return null;
    }


    public static T?[] FindObjectsOfType<T>() where T : Resource
    {
        List<T> objects = new();
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
    }*/


    /*public static EngineObject Instantiate(EngineObject obj, bool keepAssetID = false)
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
    }*/
}
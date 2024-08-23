using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public static class ResourceID
{
    /// <summary>
    /// The next ID to be assigned to a resource.
    /// Starts at 1, because the ObjectID buffer (in G-Buffer) uses 0 as a "null" value.
    /// </summary>
    private static int nextID = 1;
    
    
    public static int Generate()
    {
        int id = Interlocked.Increment(ref nextID);
        Debug.Assert(id != int.MaxValue, "ResourceID overflow!");
        return id;
    }
}
using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public static class ResourceID
{
    private static int nextID;
    
    
    public static int Generate()
    {
        int id = Interlocked.Increment(ref nextID);
        Debug.Assert(id != int.MaxValue, "ResourceID overflow!");
        return id;
    }
}
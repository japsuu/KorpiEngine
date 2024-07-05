using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public static class EntityID
{
    private static int nextID;
    
    
    public static int Generate()
    {
        int id = Interlocked.Increment(ref nextID);
        Debug.Assert(id != int.MaxValue, "EntityID overflow!");
        return id;
    }
}
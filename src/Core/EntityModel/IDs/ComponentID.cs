using System.Diagnostics;

namespace KorpiEngine.EntityModel.IDs;

public static class ComponentID
{
    private static int nextID;

    
    public static int Generate()
    {
        int id = Interlocked.Increment(ref nextID);
        Debug.Assert(id != int.MaxValue, "ComponentID overflow!");
        return id;
    }
}
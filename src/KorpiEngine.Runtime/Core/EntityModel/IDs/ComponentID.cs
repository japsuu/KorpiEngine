using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public static class ComponentID
{
    private static ulong nextID;

    
    public static ulong Generate()
    {
        ulong id = Interlocked.Increment(ref nextID);
        Debug.Assert(id != ulong.MaxValue, "ComponentID overflow!");
        return id;
    }
}
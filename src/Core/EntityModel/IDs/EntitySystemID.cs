using System.Diagnostics;

namespace KorpiEngine.EntityModel.IDs;

public static class EntitySystemID
{
    private static ulong nextID;
    
    
    public static ulong Generate<T>() where T : IEntitySystem
    {
        ulong id = TypedIDs<T>.Bit;
        Debug.Assert(id != ulong.MaxValue, "EntitySystemID overflow!");
        return id;
    }

    // ReSharper disable once UnusedTypeParameter
    private static class TypedIDs<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ulong Bit;

        static TypedIDs()
        {
            Bit = Interlocked.Increment(ref nextID);
        }
    }
}
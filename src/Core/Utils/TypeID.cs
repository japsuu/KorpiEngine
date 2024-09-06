using KorpiEngine.Exceptions;

namespace KorpiEngine.EntityModel.IDs;

/// <summary>
/// Provides a unique ID for a type.
/// </summary>
internal static class TypeID
{
    private static ulong nextID;

    /// <summary>
    /// Provides a unique ID for a type.
    /// </summary>
    public static ulong Get<T>() where T : class
    {
        if (Interlocked.Read(ref nextID) == ulong.MaxValue)
            throw new IdOverflowException("Type ID overflow.");
        
        return TypedIDs<T>.ID;
    }

    // ReSharper disable once UnusedTypeParameter
    private static class TypedIDs<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ulong ID;

        static TypedIDs()
        {
            ID = Interlocked.Increment(ref nextID);
        }
    }
}
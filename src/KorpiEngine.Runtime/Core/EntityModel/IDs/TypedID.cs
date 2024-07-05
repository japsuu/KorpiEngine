namespace KorpiEngine.Core.EntityModel.IDs;

internal static class TypedID
{
    private static ulong bitCounter;

    public static ulong GetBit<T>() where T : class
    {
        return TypedIDs<T>.Bit;
    }

    // ReSharper disable once UnusedTypeParameter
    private static class TypedIDs<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ulong Bit;

        static TypedIDs()
        {
            Bit = bitCounter++;
        }
    }
}
namespace KorpiEngine.Networking.Utility;

/// <summary>
/// Retrieves and stores byte arrays using a pooling system.
/// </summary>
internal static class ByteArrayPool
{
    /// <summary>
    /// Stored byte arrays.
    /// </summary>
    private static readonly Queue<byte[]> ByteArrays = new();

    /// <summary>
    /// Returns a byte array which will be of at least minimum length. The returned array must manually be stored.
    /// </summary>
    public static byte[] Rent(int minimumLength)
    {
        byte[]? result = null;

        if (ByteArrays.Count > 0)
            result = ByteArrays.Dequeue();

        int doubleMinimumLength = (minimumLength * 2);
        if (result == null)
            result = new byte[doubleMinimumLength];
        else if (result.Length < minimumLength)
            Array.Resize(ref result, doubleMinimumLength);

        return result;
    }

    /// <summary>
    /// Stores a byte array for re-use.
    /// </summary>
    public static void Return(byte[] buffer)
    {
        if (ByteArrays.Count > 300)
            return;
        ByteArrays.Enqueue(buffer);
    }

}
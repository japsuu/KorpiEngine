using Korpi.Networking.LowLevel.NetStack.Serialization;

namespace Korpi.Networking.Utility;

/// <summary>
/// One-time allocation pool of <see cref="bitBuffer"/>s.
/// </summary>
internal static class BufferPool
{
    [ThreadStatic]
    private static BitBuffer? bitBuffer;


    /// <summary>
    /// Gets the thread static <see cref="bitBuffer"/>.
    /// </summary>
    public static BitBuffer GetBitBuffer()
    {
        bitBuffer ??= new BitBuffer(1024);
        
        bitBuffer.Clear();

        return bitBuffer;
    }
}
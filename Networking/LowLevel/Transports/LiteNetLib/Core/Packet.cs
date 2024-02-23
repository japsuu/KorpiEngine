using Korpi.Networking.HighLevel;
using Korpi.Networking.Utility;

namespace Korpi.Networking.LowLevel.Transports.LiteNetLib.Core;

internal readonly struct Packet
{
    public readonly int ConnectionId;
    public readonly byte[] Data;
    public readonly int Length;
    public readonly Channel Channel;

    public Packet(int connectionId, byte[] data, int length, Channel channel)
    {
        ConnectionId = connectionId;
        Data = data;
        Length = length;
        Channel = channel;
    }

    public Packet(int sender, ArraySegment<byte> segment, Channel channel, int mtu)
    {
        //Prefer to max out returned array to mtu to reduce chance of resizing.
        int arraySize = Math.Max(segment.Count, mtu);
        Data = ByteArrayPool.Rent(arraySize);
        Buffer.BlockCopy(segment.Array, segment.Offset, Data, 0, segment.Count);
        ConnectionId = sender;
        Length = segment.Count;
        Channel = channel;
    }

    /// <summary>
    /// Gets the data as an ArraySegment.
    /// The offset is always 0.
    /// </summary>
    /// <returns></returns>
    public ArraySegment<byte> GetArraySegment()
    {
        return new ArraySegment<byte>(Data, 0, Length);
    }
    
    /// <summary>
    /// Gets the data as a ReadOnlySpan.
    /// The offset is always 0.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> GetReadOnlySpan()
    {
        return new ReadOnlySpan<byte>(Data, 0, Length);
    }

    public void Dispose()
    {
        ByteArrayPool.Return(Data);
    }

}
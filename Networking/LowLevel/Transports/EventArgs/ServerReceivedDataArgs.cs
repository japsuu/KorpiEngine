using KorpiEngine.Networking.HighLevel;

namespace KorpiEngine.Networking.LowLevel.Transports.EventArgs;

/// <summary>
/// Container about data received on the server.
/// </summary>
public readonly struct ServerReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// Guaranteed to contain at least 1 byte of data (<see cref="InternalPacketType"/>).
    /// The offset is always 0.
    /// </summary>
    public readonly ArraySegment<byte> Segment; //NOTE: A ReadOnlySpan<byte> could be used also, by setting the struct to ref.

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;

    /// <summary>
    /// Connection of client which sent the data.
    /// </summary>
    public readonly int ConnectionId;


    public ServerReceivedDataArgs(ArraySegment<byte> segment, Channel channel, int connectionId)
    {
        Segment = segment;
        Channel = channel;
        ConnectionId = connectionId;
    }
}
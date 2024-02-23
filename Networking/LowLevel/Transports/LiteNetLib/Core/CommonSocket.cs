using System.Collections.Concurrent;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using Korpi.Networking.Utility;
using LiteNetLib;

namespace Korpi.Networking.LowLevel.Transports.LiteNetLib.Core;

internal abstract class CommonSocket
{
    /// <summary>
    /// NetManager for this socket.
    /// Only available if the socket is active (started).
    /// </summary>
    protected abstract NetManager? NetManager { get; }

    /// <summary>
    /// Transport controlling this socket.
    /// </summary>
    protected readonly LiteNetLibTransport Transport;

    private LocalConnectionState _connectionState = LocalConnectionState.Stopped;


    protected CommonSocket(LiteNetLibTransport transport)
    {
        Transport = transport;
    }


    /// <summary>
    /// Sets a new connection state.
    /// </summary>
    protected void SetConnectionState(LocalConnectionState connectionState, bool asServer)
    {
        if (connectionState == _connectionState)
            return;

        _connectionState = connectionState;

        if (asServer)
            Transport.HandleServerConnectionState(new ServerConnectionStateArgs(connectionState));
        else
            Transport.HandleClientConnectionState(new ClientConnectionStateArgs(connectionState));
    }


    /// <summary>
    /// Called when data is received.
    /// </summary>
    protected void OnNetworkReceiveEvent(ConcurrentQueue<Packet> queue, NetPeer fromPeer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        // Get the length of the data.
        int bytesLength = reader.AvailableBytes;

        // Prefer to max out returned array to mtu to reduce the chance of resizing.
        int arraySize = Math.Max(bytesLength, Transport.UnreliableMTU);
        byte[] data = ByteArrayPool.Rent(arraySize);
        reader.GetBytes(data, bytesLength);

        // Determine the id of the peer.
        int id = fromPeer.Id;

        // Determine the received channel.
        Channel channel = deliveryMethod == DeliveryMethod.Unreliable ? Channel.Unreliable : Channel.Reliable;

        // Construct a packet.
        Packet packet = new(id, data, bytesLength, channel);
        queue.Enqueue(packet);

        // Recycle the reader.
        reader.Recycle();
    }


    /// <summary>
    /// Returns the current ConnectionState.
    /// </summary>
    internal LocalConnectionState GetConnectionState() => _connectionState;


    /// <summary>
    /// Sends data to connectionId.
    /// </summary>
    internal void Send(ref Queue<Packet> queue, Channel channel, ArraySegment<byte> segment, int connectionId)
    {
        if (GetConnectionState() != LocalConnectionState.Started)
            return;

        //ConnectionId isn't used from client to server.
        Packet outgoing = new(connectionId, segment, channel, Transport.UnreliableMTU);
        queue.Enqueue(outgoing);
    }


    internal void PollSocket()
    {
        NetManager?.PollEvents();
    }


    /// <summary>
    /// Returns the port from the socket if active, otherwise returns null.
    /// </summary>
    internal ushort? GetPort()
    {
        if (NetManager == null || !NetManager.IsRunning)
            return null;

        int port = NetManager.LocalPort;
        if (port < 0)
            port = 0;
        else if (port > ushort.MaxValue)
            port = ushort.MaxValue;

        return (ushort)port;
    }
}
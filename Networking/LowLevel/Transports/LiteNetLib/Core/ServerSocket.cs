using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using LiteNetLib;

namespace Korpi.Networking.LowLevel.Transports.LiteNetLib.Core;

internal class ServerSocket : CommonSocket
{
    private struct RemoteConnectionEvent
    {
        public readonly bool Connected;
        public readonly int ConnectionId;


        public RemoteConnectionEvent(bool connected, int connectionId)
        {
            Connected = connected;
            ConnectionId = connectionId;
        }
    }

    private readonly string _connectionKey = string.Empty;
    private readonly object _stopLock = new();

    /// <summary>
    /// Changes to the sockets local connection state.
    /// </summary>
    private ConcurrentQueue<LocalConnectionState> _localConnectionStates = new();

    /// <summary>
    /// Inbound messages which need to be handled.
    /// </summary>
    private ConcurrentQueue<Packet> _incoming = new();

    /// <summary>
    /// Outbound messages which need to be handled.
    /// </summary>
    private Queue<Packet> _outgoing = new();

    /// <summary>
    /// ConnectionEvents which need to be handled.
    /// </summary>
    private ConcurrentQueue<RemoteConnectionEvent> _remoteConnectionEvents = new();

    /// <summary>
    /// The network manager managing the server.
    /// </summary>
    private NetManager? _netManager;

    /// <summary>
    /// Maximum number of allowed clients.
    /// </summary>
    internal int MaximumClients = 1023;

    /// <summary>
    /// IPv4 address to bind server to.
    /// </summary>
    internal string Ipv4BindAddress = "";

    /// <summary>
    /// IPv6 address to bind server to.
    /// </summary>
    internal string Ipv6BindAddress = "";

    /// <summary>
    /// IPv6 is enabled only on demand, by default LiteNetLib always listens on IPv4 AND IPv6 which causes problems if IPv6 is disabled on host.
    /// This can be the case in Linux environments.
    /// </summary>
    internal bool EnableIPv6;

    protected override NetManager? NetManager => _netManager;


    public ServerSocket(LiteNetLibTransport transport) : base(transport)
    {
    }


    ~ServerSocket()
    {
        StopConnection();
    }


    /// <summary>
    /// Gets the current ConnectionState of a remote client on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    internal RemoteConnectionState GetConnectionState(int connectionId)
    {
        NetPeer? peer = GetNetPeer(connectionId, false);
        
        if (peer == null)
            return RemoteConnectionState.Stopped;
        
        return peer.ConnectionState != ConnectionState.Connected ? RemoteConnectionState.Stopped : RemoteConnectionState.Started;
    }


    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId">The connectionId to get the address for.</param>
    /// <returns>The address of the connectionId, or an empty string if the connectionId is not found.</returns>
    internal string GetConnectionAddress(int connectionId)
    {
        if (GetConnectionState() != LocalConnectionState.Started)
        {
            const string msg = "Server socket is not started.";
            LiteNetLibTransport.Logger.Warn(msg);
            return string.Empty;
        }

        NetPeer? peer = GetNetPeer(connectionId, false);
        if (peer != null)
            return peer.Address.ToString();
        
        LiteNetLibTransport.Logger.Warn($"Connection Id {connectionId} returned a null NetPeer.");
        return string.Empty;
    }


    /// <summary>
    /// Starts the server.
    /// </summary>
    internal bool StartConnection()
    {
        if (base.GetConnectionState() != LocalConnectionState.Stopped)
            return false;

        SetConnectionState(LocalConnectionState.Starting, true);

        ResetQueues();

        Task.Run(ThreadedSocketStart);
        return true;
    }


    /// <summary>
    /// Stops the local socket.
    /// </summary>
    internal bool StopConnection()
    {
        if (NetManager == null || base.GetConnectionState() == LocalConnectionState.Stopped || base.GetConnectionState() == LocalConnectionState.Stopping)
            return false;

        _localConnectionStates.Enqueue(LocalConnectionState.Stopping);
        ThreadedSocketStop();
        return true;
    }


    /// <summary>
    /// Stops a remote client by disconnecting it from the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    internal bool StopConnection(int connectionId)
    {
        // Check if the server is running and the connectionId is valid.
        if (NetManager == null || base.GetConnectionState() != LocalConnectionState.Started)
            return false;

        NetPeer? peer = GetNetPeer(connectionId, false);
        if (peer == null)
            return false;

        try
        {
            peer.Disconnect();

            // Let LiteNetLib get the disconnect event which will enqueue a remote connection state.
            Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, connectionId));
        }
        catch
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// Iterates the Outgoing queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void IterateOutgoing()
    {
        if (GetConnectionState() != LocalConnectionState.Started || NetManager == null)
        {
            // Not started, clear outgoing.
            QueueUtils.ClearPacketQueue(ref _outgoing);
        }
        else
        {
            int count = _outgoing.Count;
            for (int i = 0; i < count; i++)
            {
                Packet outgoing = _outgoing.Dequeue();
                int connectionId = outgoing.ConnectionId;

                ArraySegment<byte> segment = outgoing.GetArraySegment();
                DeliveryMethod dm = outgoing.Channel == (byte)Channel.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;

                // If over the MTU.
                if (outgoing.Channel == Channel.Unreliable && segment.Count > Transport.UnreliableMTU)
                {
                    LiteNetLibTransport.Logger.Warn(
                        $"Server is sending of {segment.Count} length on the unreliable channel, while the MTU is only {Transport.UnreliableMTU}. " +
                        $"The channel has been changed to reliable for this send.");
                    dm = DeliveryMethod.ReliableOrdered;
                }

                // Send to all clients.
                if (connectionId == -1)
                {
                    NetManager.SendToAll(segment.Array, segment.Offset, segment.Count, dm);
                }
                // Send to one client.
                else
                {
                    NetPeer? peer = GetNetPeer(connectionId, true);

                    peer?.Send(segment.Array, segment.Offset, segment.Count, dm);
                }

                outgoing.Dispose();
            }
        }
    }


    /// <summary>
    /// Iterates the Incoming queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void IterateIncoming()
    {
        /* Run local connection states first so we can begin
         * to read for data at the start of the frame, as that's
         * where incoming is read. */
        while (_localConnectionStates.TryDequeue(out LocalConnectionState result))
            SetConnectionState(result, true);

        // Not yet started.
        LocalConnectionState localState = base.GetConnectionState();
        if (localState != LocalConnectionState.Started)
        {
            ResetQueues();

            // If stopped try to kill task.
            if (localState == LocalConnectionState.Stopped)
            {
                ThreadedSocketStop();
                return;
            }
        }

        // Handle connection and disconnection events.
        while (_remoteConnectionEvents.TryDequeue(out RemoteConnectionEvent connectionEvent))
        {
            RemoteConnectionState state = connectionEvent.Connected ? RemoteConnectionState.Started : RemoteConnectionState.Stopped;
            Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(state, connectionEvent.ConnectionId));
        }

        // Handle packets.
        while (_incoming.TryDequeue(out Packet incoming))
        {
            // Make sure peer is still connected.
            NetPeer? peer = GetNetPeer(incoming.ConnectionId, true);
            if (peer != null)
            {
                ServerReceivedDataArgs dataArgs = new(
                    incoming.GetArraySegment(),
                    incoming.Channel,
                    incoming.ConnectionId);

                Transport.HandleServerReceivedPacketArgs(dataArgs);
            }

            incoming.Dispose();
        }
    }


    /// <summary>
    /// Sends a packet to a single, or all clients.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SendToClient(Channel channel, ArraySegment<byte> segment, int connectionId)
    {
        Send(ref _outgoing, channel, segment, connectionId);
    }


    /// <summary>
    /// Resets queues.
    /// </summary>
    private void ResetQueues()
    {
        QueueUtils.ClearGenericQueue(ref _localConnectionStates);
        QueueUtils.ClearPacketQueue(ref _incoming);
        QueueUtils.ClearPacketQueue(ref _outgoing);
        QueueUtils.ClearGenericQueue(ref _remoteConnectionEvents);
    }


    /// <summary>
    /// Called when a peer disconnects or times out.
    /// </summary>
    private void OnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _remoteConnectionEvents.Enqueue(new RemoteConnectionEvent(false, peer.Id));
    }


    /// <summary>
    /// Called when a peer completes connection.
    /// </summary>
    private void OnPeerConnectedEvent(NetPeer peer)
    {
        _remoteConnectionEvents.Enqueue(new RemoteConnectionEvent(true, peer.Id));
    }


    /// <summary>
    /// Called when data is received from a peer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnNetworkReceiveEvent(NetPeer fromPeer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        // If over the MTU.
        if (reader.AvailableBytes > Transport.UnreliableMTU)
        {
            _remoteConnectionEvents.Enqueue(new RemoteConnectionEvent(false, fromPeer.Id));
            fromPeer.Disconnect();
            LiteNetLibTransport.Logger.Warn(
                $"Server received a packet of {reader.AvailableBytes} length on the unreliable channel, while the MTU is only {Transport.UnreliableMTU}. " +
                $"The peer has been disconnected.");
        }
        else
        {
            OnNetworkReceiveEvent(_incoming, fromPeer, reader, deliveryMethod);
        }
    }


    /// <summary>
    /// Called when a remote connection request is made.
    /// </summary>
    private void OnConnectionRequestEvent(ConnectionRequest request)
    {
        if (NetManager == null)
            return;

        // If at maximum peers.
        if (NetManager.ConnectedPeersCount >= MaximumClients)
        {
            request.Reject();
            return;
        }

        request.AcceptIfKey(_connectionKey);
    }


    /// <summary>
    /// Gets the NetPeer for a connectionId.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get NetPeer for.</param>
    /// <param name="connectedOnly">Whether to only return connected peers.</param>
    /// <returns></returns>
    private NetPeer? GetNetPeer(int connectionId, bool connectedOnly)
    {
        if (NetManager == null)
            return null;
        
        NetPeer? peer = NetManager.GetPeerById(connectionId);
        if (connectedOnly && peer != null && peer.ConnectionState != ConnectionState.Connected)
            peer = null;

        return peer;
    }


    /// <summary>
    /// Threaded operation to start the server.
    /// </summary>
    private void ThreadedSocketStart()
    {
        EventBasedNetListener listener = new();
        listener.ConnectionRequestEvent += OnConnectionRequestEvent;
        listener.PeerConnectedEvent += OnPeerConnectedEvent;
        listener.NetworkReceiveEvent += OnNetworkReceiveEvent;
        listener.PeerDisconnectedEvent += OnPeerDisconnectedEvent;
        
        _netManager = new NetManager(listener, Transport.ExtraPacketLayer)
        {
            DontRoute = Transport.DoNotRoute,
            DisconnectTimeout = Transport.TimeoutMilliseconds,
            MtuOverride = Transport.UnreliableMTUFragmented
        };
        
        IPAddress? ipv4;
        IPAddress? ipv6;

        if (!string.IsNullOrEmpty(Ipv4BindAddress))
        {
            if (!IPAddress.TryParse(Ipv4BindAddress, out ipv4!))
                ipv4 = null;

            if (ipv4 == null)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Ipv4BindAddress);
                if (hostEntry.AddressList.Length > 0)
                {
                    ipv4 = hostEntry.AddressList[0];
                    LiteNetLibTransport.Logger.Warn($"IPv4 could not be parsed correctly, but was resolved to {ipv4}");
                }
            }
        }
        else
        {
            ipv4 = IPAddress.Any;
        }

        if (EnableIPv6 && !string.IsNullOrEmpty(Ipv6BindAddress))
        {
            if (!IPAddress.TryParse(Ipv6BindAddress, out ipv6!))
            {
                LiteNetLibTransport.Logger.Warn("IPv6 could not parse correctly, so IPv6 will be disabled.");
                ipv6 = null;
                EnableIPv6 = false;
            }
        }
        else
        {
            ipv6 = IPAddress.IPv6Any;
        }

        bool ipv4Failed = ipv4 == null;
        bool ipv6Failed = EnableIPv6 && ipv6 == null;

        if (ipv4Failed || ipv6Failed)
        {
            LiteNetLibTransport.Logger.Error(
                ipv4Failed
                    ? $"IPv4 address {Ipv4BindAddress} failed to parse. Clear the bind address field to use any bind address."
                    : $"IPv6 address {Ipv6BindAddress} failed to parse. Clear the bind address field to use any bind address.");
            StopConnection();
            return;
        }

        bool wasStartSuccess = _netManager.Start(ipv4, ipv6, Transport.Port);

        if (wasStartSuccess)
        {
            _localConnectionStates.Enqueue(LocalConnectionState.Started);
        }
        else
        {
            LiteNetLibTransport.Logger.Error($"Server failed to start. Is the specified port ({Transport.Port}) unavailable?");
            StopConnection();
        }
    }


    /// <summary>
    /// Socket operation to stop the server.
    /// </summary>
    private void ThreadedSocketStop()
    {
        if (_netManager == null)
            return;
        
        Task.Run(
            () =>
            {
                lock (_stopLock)
                {
                    _netManager.Stop();
                    _netManager = null;
                }

                // If not stopped yet also enqueue stop.
                if (GetConnectionState() != LocalConnectionState.Stopped)
                    _localConnectionStates.Enqueue(LocalConnectionState.Stopped);
            });
    }
}
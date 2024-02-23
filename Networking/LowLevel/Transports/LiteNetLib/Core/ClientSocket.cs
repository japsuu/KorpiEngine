using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using LiteNetLib;

namespace Korpi.Networking.LowLevel.Transports.LiteNetLib.Core;

internal class ClientSocket : CommonSocket
{
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
    /// Locks the NetManager to stop it.
    /// </summary>
    private readonly object _stopLock = new();
    
    /// <summary>
    /// The network manager managing the client.
    /// </summary>
    private NetManager? _netManager;

    /// <summary>
    /// Address to connect to.
    /// </summary>
    internal string Address = "localhost";
    
    protected override NetManager? NetManager => _netManager;


    public ClientSocket(LiteNetLibTransport transport) : base(transport)
    {
    }
    
    
    ~ClientSocket()
    {
        StopConnection();
    }


    /// <summary>
    /// Threaded operation to start the socket.
    /// </summary>
    private void ThreadedSocketStart()
    {
        EventBasedNetListener listener = new();
        listener.NetworkReceiveEvent += OnNetworkReceiveEvent;
        listener.PeerConnectedEvent += OnPeerConnectedEvent;
        listener.PeerDisconnectedEvent += OnPeerDisconnectedEvent;
        
        _netManager = new NetManager(listener, Transport.ExtraPacketLayer)
        {
            DontRoute = Transport.DoNotRoute,
            DisconnectTimeout = Transport.TimeoutMilliseconds,
            MtuOverride = Transport.UnreliableMTUFragmented
        };
        
        _localConnectionStates.Enqueue(LocalConnectionState.Starting);
        _netManager.Start();
        _netManager.Connect(Address, Transport.Port, string.Empty);
    }


    /// <summary>
    /// Threaded operation to stop the socket.
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


    /// <summary>
    /// Starts the client connection.
    /// </summary>
    internal bool StartConnection()
    {
        if (GetConnectionState() != LocalConnectionState.Stopped)
            return false;

        SetConnectionState(LocalConnectionState.Starting, false);

        ResetQueues();
        Task.Run(ThreadedSocketStart);

        return true;
    }


    /// <summary>
    /// Stops the client socket.
    /// </summary>
    internal bool StopConnection(DisconnectInfo? info = null)
    {
        if (GetConnectionState() == LocalConnectionState.Stopped || GetConnectionState() == LocalConnectionState.Stopping)
            return false;

        if (info != null)
            LiteNetLibTransport.Logger.Info($"Local client disconnect reason: {info.Value.Reason}.");

        SetConnectionState(LocalConnectionState.Stopping, false);
        ThreadedSocketStop();
        return true;
    }


    /// <summary>
    /// Resets queues.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetQueues()
    {
        QueueUtils.ClearGenericQueue(ref _localConnectionStates);
        QueueUtils.ClearPacketQueue(ref _incoming);
        QueueUtils.ClearPacketQueue(ref _outgoing);
    }


    /// <summary>
    /// Called when disconnected from the server.
    /// </summary>
    private void OnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        StopConnection(disconnectInfo);
    }


    /// <summary>
    /// Called when connected to the server.
    /// </summary>
    private void OnPeerConnectedEvent(NetPeer peer)
    {
        _localConnectionStates.Enqueue(LocalConnectionState.Started);
    }


    /// <summary>
    /// Called when data is received from a peer.
    /// </summary>
    private void OnNetworkReceiveEvent(NetPeer fromPeer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        OnNetworkReceiveEvent(_incoming, fromPeer, reader, deliveryMethod);
    }


    /// <summary>
    /// Dequeues and processes outgoing.
    /// </summary>
    private void DequeueOutgoing()
    {
        NetPeer? peer = null;
        
        if (_netManager != null)
            peer = _netManager.FirstPeer;

        // Server connection hasn't been made.
        if (peer == null)
        {
            /* Only dequeue outgoing because other queues might have
             * relevant information, such as the local connection queue. */
            QueueUtils.ClearPacketQueue(ref _outgoing);
        }
        else
        {
            int count = _outgoing.Count;
            for (int i = 0; i < count; i++)
            {
                Packet outgoing = _outgoing.Dequeue();

                ArraySegment<byte> segment = outgoing.GetArraySegment();
                DeliveryMethod dm = outgoing.Channel == (byte)Channel.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;

                //If over the MTU.
                if (outgoing.Channel == Channel.Unreliable && segment.Count > Transport.UnreliableMTU)
                {
                    LiteNetLibTransport.Logger.Warn(
                        $"Client is sending of {segment.Count} length on the unreliable channel, while the MTU is only {Transport.UnreliableMTU}. The channel has been changed to reliable for this send.");
                    dm = DeliveryMethod.ReliableOrdered;
                }

                peer.Send(segment.Array, segment.Offset, segment.Count, dm);

                outgoing.Dispose();
            }
        }
    }


    /// <summary>
    /// Allows for Outgoing queue to be iterated.
    /// </summary>
    internal void IterateOutgoing()
    {
        DequeueOutgoing();
    }


    /// <summary>
    /// Iterates the Incoming queue.
    /// </summary>
    internal void IterateIncoming()
    {
        /* Run local connection states first so we can begin
         * to read for data at the start of the frame, as that's
         * where incoming is read. */
        while (_localConnectionStates.TryDequeue(out LocalConnectionState result))
            SetConnectionState(result, false);

        //Not yet started, cannot continue.
        LocalConnectionState localState = GetConnectionState();
        if (localState != LocalConnectionState.Started)
        {
            ResetQueues();

            //If stopped try to kill task.
            if (localState == LocalConnectionState.Stopped)
            {
                ThreadedSocketStop();
                return;
            }
        }

        /* Incoming. */
        while (_incoming.TryDequeue(out Packet incoming))
        {
            ClientReceivedDataArgs dataArgs = new(
                incoming.GetArraySegment(),
                incoming.Channel);
            Transport.HandleClientReceivedPacketArgs(dataArgs);

            //Dispose of packet.
            incoming.Dispose();
        }
    }


    /// <summary>
    /// Sends a packet to the server.
    /// </summary>
    internal void SendToServer(Channel channel, ArraySegment<byte> segment)
    {
        //Not started, cannot send.
        if (GetConnectionState() != LocalConnectionState.Started)
            return;

        Send(ref _outgoing, channel, segment, -1);
    }
}
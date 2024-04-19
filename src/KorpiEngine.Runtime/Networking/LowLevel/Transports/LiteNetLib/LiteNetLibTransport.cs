using System.Runtime.CompilerServices;
using KorpiEngine.Core.Logging;
using KorpiEngine.Networking.HighLevel;
using KorpiEngine.Networking.HighLevel.Connections;
using KorpiEngine.Networking.LowLevel.Transports.EventArgs;
using KorpiEngine.Networking.LowLevel.Transports.LiteNetLib.Core;
using KorpiEngine.Networking.LowLevel.Transports.LiteNetLib.Core.LiteNetLib;
using KorpiEngine.Networking.LowLevel.Transports.LiteNetLib.Core.LiteNetLib.Layers;

namespace KorpiEngine.Networking.LowLevel.Transports.LiteNetLib;

public class LiteNetLibTransport : Transport
{
    internal static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(LiteNetLibTransport));

    /// <summary>
    /// Minimum UDP packet size allowed.
    /// </summary>
    private const int MINIMUM_UDP_MTU = 576;

    /// <summary>
    /// Maximum UDP packet size allowed.
    /// </summary>
    private const int MAXIMUM_UDP_MTU = 1023;

    /// <summary>
    /// The extra intermediary PacketLayer to use with LiteNetLib.
    /// Provides extra processing of packages, like CRC checksum or encryption.
    /// </summary>
    internal PacketLayerBase? ExtraPacketLayer;

    /// <summary>
    /// The port to bind the server to.
    /// </summary>
    internal ushort Port;

    /// <summary>
    /// While true, force sockets to send data directly to network interface, without routing.
    /// </summary>
    internal bool DoNotRoute;

    /// <summary>
    /// Maximum transmission unit for the unreliable channel.
    /// </summary>
    internal int UnreliableMTU;

    /// <summary>
    /// The maximum transmission unit for the unreliable channel, including the fragment header.
    /// </summary>
    internal int UnreliableMTUFragmented => NetConstants.FragmentedHeaderTotalSize + UnreliableMTU;
    
    /// <summary>
    /// How long in milliseconds until a client times from server.
    /// </summary>
    internal int TimeoutMilliseconds;

    /// <summary>
    /// The server socket.
    /// Only set after StartLocalConnection is called.
    /// </summary>
    private readonly ServerSocket _server;

    /// <summary>
    /// The client socket.
    /// Only set after StartLocalConnection is called.
    /// </summary>
    private readonly ClientSocket _client;


    public LiteNetLibTransport(ushort port = 7531, int maxClients = 4095, int unreliableMtu = 1023, ushort timeoutMilliseconds = 30000, bool enableIPv6 = false, bool doNotRoute = false, PacketLayerBase? extraPacketLayer = null)
    {
        Port = port;
        DoNotRoute = doNotRoute;
        ExtraPacketLayer = extraPacketLayer;
        
        int clampedMtu = Math.Clamp(unreliableMtu, MINIMUM_UDP_MTU, MAXIMUM_UDP_MTU);
        if (clampedMtu != unreliableMtu)
            Logger.Warn($"UnreliableMtu was clamped to {clampedMtu} from {unreliableMtu}.");
        UnreliableMTU = clampedMtu;
        
        TimeoutMilliseconds = timeoutMilliseconds == 0 ? int.MaxValue : Math.Min(int.MaxValue, timeoutMilliseconds);
        
        _server = new ServerSocket(this);
        _client = new ClientSocket(this);
        _server.MaximumClients = maxClients;
        _server.EnableIPv6 = enableIPv6;
    }


    ~LiteNetLibTransport()
    {
        Shutdown();
    }


    #region Connection State

    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId">The connection Id to get the address for.</param>
    /// <returns>The address of the remote connection Id or an empty string if the connection Id is not found.</returns>
    public override string GetRemoteConnectionAddress(int connectionId)
    {
        return _server.GetConnectionAddress(connectionId);
    }


    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public override event Action<ClientConnectionStateArgs>? LocalClientConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public override event Action<ServerConnectionStateArgs>? LocalServerConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public override event Action<RemoteConnectionStateArgs>? RemoteClientConnectionStateChanged;


    /// <summary>
    /// Gets the current local ConnectionState.
    /// </summary>
    /// <param name="server">True if getting ConnectionState for the server.</param>
    public override LocalConnectionState GetLocalConnectionState(bool server)
    {
        return server ? _server.GetConnectionState() : _client.GetConnectionState();
    }


    /// <summary>
    /// Gets the current ConnectionState of a remote client on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    public override RemoteConnectionState GetRemoteConnectionState(int connectionId)
    {
        return _server.GetConnectionState(connectionId);
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for the local client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
    {
        LocalClientConnectionStateChanged?.Invoke(connectionStateArgs);
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for the local server.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
    {
        LocalServerConnectionStateChanged?.Invoke(connectionStateArgs);
    }


    /// <summary>
    /// Handles a ConnectionStateArgs for a remote client.
    /// </summary>
    /// <param name="connectionStateArgs"></param>
    public void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
    {
        RemoteClientConnectionStateChanged?.Invoke(connectionStateArgs);
    }

    #endregion

    #region Iterating Sockets

    /// <summary>
    /// Called every update to poll sockets for incoming data.
    /// Should be called every frame if possible.
    /// </summary>
    public override void PollSockets()
    {
        _server.PollSocket();
        _client.PollSocket();
    }


    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public override void IterateIncomingData(bool server)
    {
        if (server)
            _server.IterateIncoming();
        else
            _client.IterateIncoming();
    }


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="server">True to process data received on the server.</param>
    public override void IterateOutgoingData(bool server)
    {
        if (server)
            _server.IterateOutgoing();
        else
            _client.IterateOutgoing();
    }

    #endregion

    #region Receiving Data

    /// <summary>
    /// Called when client receives data.
    /// </summary>
    public override event Action<ClientReceivedDataArgs>? LocalClientReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs"></param>
    public void HandleClientReceivedPacketArgs(ClientReceivedDataArgs receivedDataArgs)
    {
        LocalClientReceivedPacket?.Invoke(receivedDataArgs);
    }


    /// <summary>
    /// Called when server receives data.
    /// </summary>
    public override event Action<ServerReceivedDataArgs>? LocalServerReceivedPacket;


    /// <summary>
    /// Handles a ClientReceivedDataArgs.
    /// </summary>
    /// <param name="receivedDataArgs"></param>
    public void HandleServerReceivedPacketArgs(ServerReceivedDataArgs receivedDataArgs)
    {
        LocalServerReceivedPacket?.Invoke(receivedDataArgs);
    }

    #endregion

    #region Sending Data

    /// <summary>
    /// Sends to the server or all clients.
    /// </summary>
    /// <param name="channel">Channel to use.</param>
    /// <param name="segment">Data to send.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToServer(Channel channel, ArraySegment<byte> segment)
    {
        _client.SendToServer(channel, segment);
    }


    /// <summary>
    /// Sends data to a client.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="segment"></param>
    /// <param name="connectionId"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SendToClient(Channel channel, ArraySegment<byte> segment, int connectionId)
    {
        _server.SendToClient(channel, segment, connectionId);
    }

    #endregion

    #region Configuration
    
    public override int GetTimeout(bool asServer) => TimeoutMilliseconds;


    public override int GetMaximumClients()
    {
        return _server.MaximumClients;
    }


    public override void SetMaximumClients(int value)
    {
        _server.MaximumClients = value;
    }


    public override void SetClientConnectAddress(string address)
    {
        _client.Address = address;
    }


    public override string GetClientConnectAddress() => _client.Address;


    public override void SetServerBindAddress(AddressType type, string address)
    {
        switch (type)
        {
            case AddressType.IPV4:
                _server.Ipv4BindAddress = address;
                break;
            case AddressType.IPV6:
                _server.Ipv6BindAddress = address;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }


    public override string GetServerBindAddress(AddressType type)
    {
        return type switch
        {
            AddressType.IPV4 => _server.Ipv4BindAddress,
            AddressType.IPV6 => _server.Ipv6BindAddress,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }


    public override void SetPort(ushort port)
    {
        Port = port;
    }


    /// <summary>
    /// Gets which port to use.
    /// Tries to first get the port of the running server socket, then the client socket.
    /// If neither are running, returns the port they would use when started.
    /// </summary>
    public override ushort GetPort()
    {
        // First try to get the port of the running server socket.
        ushort? serverPort = _server.GetPort();
        if (serverPort.HasValue)
            return serverPort.Value;
        
        // Then the client socket.
        ushort? clientPort = _client.GetPort();
        if (clientPort.HasValue)
            return clientPort.Value;
        
        // If neither are running, return the port they would use.
        return Port;
    }

    #endregion

    #region Starting and Stopping

    /// <summary>
    /// Starts the local server or client using configured settings.
    /// </summary>
    /// <param name="server">True to start server.</param>
    public override bool StartLocalConnection(bool server)
    {
        return server ? StartServer() : StartClient();
    }


    /// <summary>
    /// Stops the local server or client.
    /// </summary>
    /// <param name="server">True to stop server.</param>
    public override bool StopLocalConnection(bool server)
    {
        return server ? StopServer() : StopClient();
    }


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    /// <param name="immediately">True to abruptly stop the client socket. 
    /// The technique used to accomplish immediate disconnects may vary depending on the transport.</param>
    public override bool StopRemoteConnection(int connectionId, bool immediately)
    {
        if (_server == null)
            throw new InvalidOperationException("Cannot stop remote connection when server is not started.");
        
        return _server.StopConnection(connectionId);
    }


    /// <summary>
    /// Stops both client and server.
    /// </summary>
    public override void Shutdown()
    {
        //Stops client then server connections.
        StopLocalConnection(false);
        StopLocalConnection(true);
    }


    public override int GetUnreliableMTU() => UnreliableMTU;


    /// <summary>
    /// Starts server.
    /// </summary>
    private bool StartServer()
    {
        return _server.StartConnection();
    }


    /// <summary>
    /// Starts the client.
    /// </summary>
    private bool StartClient()
    {
        return _client.StartConnection();
    }


    /// <summary>
    /// Stops server.
    /// </summary>
    private bool StopServer()
    {
        return _server.StopConnection();
    }


    /// <summary>
    /// Stops the client.
    /// </summary>
    private bool StopClient()
    {
        return _client.StopConnection();
    }

    #endregion
}
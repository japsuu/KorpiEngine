using Common.Logging;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Authentication;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.HighLevel.Messages;
using Korpi.Networking.HighLevel.Messages.Handlers;
using Korpi.Networking.LowLevel;
using Korpi.Networking.LowLevel.NetStack.Serialization;
using Korpi.Networking.LowLevel.Transports;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using Korpi.Networking.Utility;

namespace Korpi.Networking;

/// <summary>
/// Manages the network server.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetServerManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetServerManager));
    private readonly Dictionary<ushort, MessageHandlerCollection> _messageHandlers = new(); // Registered message handlers by message ID.
    private readonly NetworkManager _netManager;
    private readonly TransportManager _transportManager;

    private Authenticator? _authenticator;

    /// <summary>
    /// True if the server connection has started.
    /// </summary>
    public bool Started { get; private set; }

    /// <summary>
    /// Authenticated and non-authenticated connected clients, by clientId.
    /// </summary>
    public readonly Dictionary<int, NetworkConnection> Clients = new();

    /// <summary>
    /// Called after the local server connection state changes.
    /// </summary>  //BUG: Do not use the low-level args, create a high-level args class instead.
    public event Action<ServerConnectionStateArgs>? ConnectionStateChanged;

    /// <summary>
    /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
    /// </summary>
    public event Action<NetworkConnection, bool>? ClientAuthResultReceived;

    /// <summary>
    /// Called when a remote client connects to the server.
    /// </summary>
    public event Action<NetworkConnection>? RemoteClientConnected;

    /// <summary>
    /// Called when a remote client disconnects from the server.
    /// </summary>
    public event Action<NetworkConnection>? RemoteClientDisconnected;

    /// <summary>
    /// Called when a client is removed from the server using <see cref="Kick(NetworkConnection,KickReason)"/>. This is invoked before the client is disconnected.
    /// NetworkConnection (when available), clientId, and KickReason are provided.
    /// </summary>
    public event Action<NetworkConnection?, int, KickReason>? ClientKicked;


    /// <summary>
    /// Creates a new server manager using the specified transport.
    /// </summary>
    /// <param name="netManager">The network manager owning this server manager.</param>
    /// <param name="transportManager">The transport to use.</param>
    public NetServerManager(NetworkManager netManager, TransportManager transportManager)
    {
        _netManager = netManager;
        _transportManager = transportManager;
        _transportManager.Transport.LocalServerReceivedPacket += OnLocalServerReceivePacket;
        _transportManager.Transport.LocalServerConnectionStateChanged += OnLocalServerConnectionStateChanged;
        _transportManager.Transport.RemoteClientConnectionStateChanged += OnRemoteClientConnectionStateChanged;
    }


    /// <summary>
    /// Sets the maximum number of connections the server can have.
    /// </summary>
    public void SetMaxConnections(int maxConnections)
    {
        _transportManager.SetMaximumClients(maxConnections);
    }


    /// <summary>
    /// Assigns an authenticator to the server.
    /// </summary>
    /// <param name="authenticator">The authenticator to use.</param>
    public void SetAuthenticator(Authenticator? authenticator)
    {
        _authenticator = authenticator;
        if (_authenticator == null)
            return;

        _authenticator.Initialize(_netManager);
        _authenticator.ConcludedAuthenticationResult += OnAuthenticatorConcludeResult;
    }


    /// <summary>
    /// Starts the server with the specified address and port.
    /// </summary>
    /// <param name="address">The address to bind to.</param>
    /// <param name="port">The port to bind to.</param>
    public void StartServer(string address, ushort port)
    {
        _transportManager.SetServerBindAddress(address);
        _transportManager.SetPort(port);
        _transportManager.StartConnection(true);
    }


    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <param name="sendDisconnectionPackets">True to send a disconnect message to all clients before stopping the server.</param>
    public void StopServer(bool sendDisconnectionPackets)
    {
        if (sendDisconnectionPackets)
            SendDisconnectPackets(Clients.Values.ToList(), true);

        _transportManager.StopConnection(true);
    }


    /// <summary>
    /// Registers a method to call when a message of the specified type arrives.
    /// </summary>
    /// <param name="handler">Method to call.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to send this message.</param>
    /// <typeparam name="T"></typeparam>
    public void RegisterPacketHandler<T>(Action<NetworkConnection, T, Channel> handler, bool requireAuthenticated = true) where T : NetMessage
    {
        ushort key = MessageManager.MessageIdCache.GetId<T>();

        if (!_messageHandlers.TryGetValue(key, out MessageHandlerCollection? packetHandler))
        {
            packetHandler = new ClientMessageHandler<T>(requireAuthenticated);
            _messageHandlers.Add(key, packetHandler);
        }

        packetHandler.RegisterHandler(handler);
    }


    /// <summary>
    /// Unregisters a method call from a message type.
    /// </summary>
    /// <param name="handler">Method to unregister.</param>
    /// <typeparam name="T">Type of message being unregistered.</typeparam>
    public void UnregisterPacketHandler<T>(Action<NetworkConnection, T, Channel> handler) where T : NetMessage
    {
        ushort key = MessageManager.MessageIdCache.GetId<T>();
        if (_messageHandlers.TryGetValue(key, out MessageHandlerCollection? packetHandler))
            packetHandler.UnregisterHandler(handler);
    }


    /// <summary>
    /// Sends a message to a connection.
    /// </summary>
    /// <param name="connection">Connection to send to.</param>
    /// <param name="message">Message being sent.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to receive this message.</param>
    /// <param name="channel">Channel to send on.</param>
    /// <typeparam name="T">Type of message to send.</typeparam>
    public void SendMessageToClient<T>(NetworkConnection connection, T message, bool requireAuthenticated = true, Channel channel = Channel.Reliable)
        where T : NetMessage
    {
        if (!Started)
        {
            Logger.Warn($"Cannot send message {message} to client because server is not active.");
            return;
        }

        if (!connection.IsActive)
        {
            Logger.Warn("Connection is not active, cannot send message.");
            return;
        }

        if (requireAuthenticated && !connection.IsAuthenticated)
        {
            Logger.Warn($"Cannot send message {message} to client {connection.ClientId} because they are not authenticated.");
            return;
        }

        // Write the packet.
        BitBuffer buffer = BufferPool.GetBitBuffer();
        buffer.AddByte((byte)InternalPacketType.Message);
        message.Serialize(buffer);

        // Copy the buffer to a byte array.
        byte[] byteBuffer = ByteArrayPool.Rent(buffer.Length);
        int length = buffer.ToArray(byteBuffer);
        ArraySegment<byte> segment = new(byteBuffer, 0, length);
        
        Logger.Debug($"Sending message {message} to client {connection.ClientId}.");

        // Send the packet.
        _transportManager.SendToClient(channel, segment, connection.ClientId);
        ByteArrayPool.Return(byteBuffer);
    }


    /// <summary>
    /// Sends a message to all clients.
    /// </summary>
    /// <param name="message">Packet data being sent.</param>
    /// <param name="requireAuthenticated">True if the client must be authenticated to receive this message.</param>
    /// <param name="channel">Channel to send on.</param>
    /// <typeparam name="T">The type of message to send.</typeparam>
    public void SendMessageToAllClients<T>(T message, bool requireAuthenticated = true, Channel channel = Channel.Reliable) where T : NetMessage
    {
        if (!Started)
        {
            Logger.Warn("Cannot send message to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
            SendMessageToClient(c, message, requireAuthenticated, channel);
    }


    /// <summary>
    /// Sends a message to all clients except the specified one.
    /// </summary>
    public void SendMessageToAllClientsExcept<T>(T message, NetworkConnection except, bool requireAuthenticated = true, Channel channel = Channel.Reliable)
        where T : NetMessage
    {
        if (!Started)
        {
            Logger.Warn("Cannot send message to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
        {
            if (c == except)
                continue;
            SendMessageToClient(c, message, requireAuthenticated, channel);
        }
    }


    /// <summary>
    /// Sends a message to all clients except the specified ones.
    /// </summary>
    public void SendMessageToAllClientsExcept<T>(T message, List<NetworkConnection> except, bool requireAuthenticated = true,
        Channel channel = Channel.Reliable) where T : NetMessage
    {
        if (!Started)
        {
            Logger.Warn("Cannot send message to clients because server is not active.");
            return;
        }

        foreach (NetworkConnection c in Clients.Values)
        {
            if (except.Contains(c))
                continue;
            SendMessageToClient(c, message, requireAuthenticated, channel);
        }
    }


    /// <summary>
    /// Kicks a connection immediately while invoking ClientKicked.
    /// </summary>
    /// <param name="conn">Client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(NetworkConnection conn, KickReason kickReason)
    {
        if (!conn.IsValid)
            return;

        ClientKicked?.Invoke(conn, conn.ClientId, kickReason);
        if (conn.IsActive)
            conn.Disconnect(true);
    }


    /// <summary>
    /// Kicks a connection immediately while invoking ClientKicked.
    /// </summary>
    /// <param name="connId">Id of the client to kick.</param>
    /// <param name="kickReason">Reason client is being kicked.</param>
    public void Kick(int connId, KickReason kickReason)
    {
        ClientKicked?.Invoke(null, connId, kickReason);

        _transportManager.StopConnection(connId, true);
    }


    /// <summary>
    /// Handles a received message.
    /// </summary>
    private void OnLocalServerReceivePacket(ServerReceivedDataArgs args)
    {
        Logger.Verbose($"Received segment {args.Segment.AsStringHex()} from client {args.ConnectionId}.");
        // Not from a valid connection.
        if (args.ConnectionId < 0)
        {
            Logger.Warn($"Received a message from an unknown connection with id {args.ConnectionId}. Ignoring.");
            return;
        }

        if (!Clients.TryGetValue(args.ConnectionId, out NetworkConnection? connection))
        {
            Logger.Warn($"ConnectionId {args.ConnectionId} not found within Clients. Connection will be kicked immediately.");
            Kick(args.ConnectionId, KickReason.UnexpectedProblem);
            return;
        }

        if (args.Segment.Array == null)
        {
            Logger.Warn($"Received a message with null data. Kicking client {args.ConnectionId} immediately.");
            Kick(connection, KickReason.MalformedData);
            return;
        }

        //TODO: Kick the client immediately if message is over MTU.

        BitBuffer buffer = BufferPool.GetBitBuffer();
        buffer.FromArray(args.Segment.Array, args.Segment.Count);
        InternalPacketType packetType = (InternalPacketType)buffer.ReadByte();

        switch (packetType)
        {
            case InternalPacketType.Unset:
                Logger.Warn("Received a packet with an unset type. Kicking client immediately.");
                Kick(connection, KickReason.MalformedData);
                break;
            case InternalPacketType.Welcome:
                Logger.Warn("Received a welcome packet from a client. They might be trying to exploit the server. Kicking immediately.");
                Kick(connection, KickReason.ExploitAttempt);
                break;
            case InternalPacketType.Message:
                ParseMessage(connection, buffer, args.Channel);
                break;
            case InternalPacketType.Disconnect:
                Logger.Info($"Received a disconnect message from client {args.ConnectionId}. Kicking client immediately.");
                Kick(connection, KickReason.Unset);
                break;
            default:
                Logger.Warn($"Received a message with an unknown packet type {packetType}. Kicking client {args.ConnectionId} immediately.");
                Kick(connection, KickReason.MalformedData);
                break;
        }
    }


    private void ParseMessage(NetworkConnection conn, BitBuffer buffer, Channel channel)
    {
        ushort id = buffer.ReadUShort();
        NetMessage netMessage = MessageManager.MessageTypeCache.CreateInstance(id);
        netMessage.Deserialize(buffer);

        if (!_messageHandlers.TryGetValue(id, out MessageHandlerCollection? packetHandler))
        {
            Logger.Warn($"Received a {netMessage} but no handler is registered for it. Ignoring.");
            return;
        }

        if (packetHandler.RequireAuthentication && !conn.IsAuthenticated)
        {
            Logger.Warn($"Client {conn.ClientId} sent a message of type {netMessage} without being authenticated. Kicking.");
            Kick(conn, KickReason.ExploitAttempt);
            return;
        }
        
        Logger.Debug($"Received message {netMessage} from client {conn.ClientId}.");

        packetHandler.InvokeHandlers(conn, netMessage, channel);
    }


    /// <summary>
    /// Called when the local server connection state changes.
    /// </summary>
    /// <param name="args"></param>
    private void OnLocalServerConnectionStateChanged(ServerConnectionStateArgs args)
    {
        LocalConnectionState state = args.ConnectionState;
        Started = state == LocalConnectionState.Started;

        if (!Started)
            NetworkManager.ClearClientsCollection(Clients);

        string tName = _transportManager.TransportTypeName;
        string socketInformation = string.Empty;
        if (state == LocalConnectionState.Starting)
            socketInformation = $" Bound to IP {_transportManager.GetServerBindAddress(AddressType.IPV4)}. Listening on port {_transportManager.GetPort()}.";
        Logger.Info($"Local server is {state.ToString().ToLower()} for {tName}.{socketInformation}");

        ConnectionStateChanged?.Invoke(args);
    }


    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    private void OnRemoteClientConnectionStateChanged(RemoteConnectionStateArgs args)
    {
        int id = args.ConnectionId;
        if (id is < 0 or > short.MaxValue)
        {
            Logger.Error($"Received an invalid connection id {id} from transport. Kicking client.");
            Kick(args.ConnectionId, KickReason.UnexpectedProblem);
            return;
        }

        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Started:
            {
                Logger.Info($"Remote connection started for clientId {id}.");
                NetworkConnection conn = new(_netManager, id, true);
                Clients.Add(args.ConnectionId, conn);
                RemoteClientConnected?.Invoke(conn);

                // Connection is no longer valid. This can occur if the user changes the state using the RemoteClientConnected event.
                if (!conn.IsValid)
                    return;

                if (_authenticator != null)
                    _authenticator.OnRemoteConnection(conn);
                else
                    ClientAuthenticated(conn);
                break;
            }
            case RemoteConnectionState.Stopped:
            {
                /* If client's connection is found then clean
                 * them up from server. */
                if (Clients.TryGetValue(id, out NetworkConnection? conn))
                {
                    conn.SetDisconnecting(true);
                    RemoteClientDisconnected?.Invoke(conn);
                    Clients.Remove(id);
                    SendClientConnectionChangePacket(false, conn);

                    conn.Dispose();
                    Logger.Info($"Remote connection stopped for clientId {id}.");
                }

                break;
            }
        }
    }


    /// <summary>
    /// Called when the authenticator has concluded a result for a connection.
    /// </summary>
    /// <param name="conn">The connection that was authenticated.</param>
    /// <param name="success">True if authentication passed, false if failed.</param>
    private void OnAuthenticatorConcludeResult(NetworkConnection conn, bool success)
    {
        if (success)
            ClientAuthenticated(conn);
        else
            conn.Disconnect(false);
    }


    /// <summary>
    /// Called when a remote client authenticates with the server.
    /// </summary>
    private void ClientAuthenticated(NetworkConnection connection)
    {
        // Immediately send connectionId to client.
        connection.SetAuthenticated();
        /* Send client Ids before telling the client
         * they are authenticated. This is important because when the client becomes
         * authenticated they set their LocalConnection using Clients field in ClientManager,
         * which is set after getting Ids. */
        SendClientConnectionChangePacket(true, connection);
        SendWelcomePacket(connection);

        ClientAuthResultReceived?.Invoke(connection, true);
    }


    /// <summary>
    /// Sends a welcome message to a client.
    /// </summary>
    /// <param name="connection"></param>
    private void SendWelcomePacket(NetworkConnection connection)
    {
        // Sanity check.
        if (!connection.IsValid)
        {
            Logger.Warn("Cannot send welcome message to client because connection is not valid.");
            return;
        }

        // Write the packet.
        BitBuffer buffer = BufferPool.GetBitBuffer();
        buffer.AddByte((byte)InternalPacketType.Welcome);
        buffer.AddUShort((ushort)connection.ClientId);

        // Copy the buffer to a byte array.
        byte[] byteBuffer = ByteArrayPool.Rent(buffer.Length);
        int length = buffer.ToArray(byteBuffer);
        ArraySegment<byte> segment = new(byteBuffer, 0, length);

        // Send the packet.
        _transportManager.SendToClient(Channel.Reliable, segment, connection.ClientId);
        ByteArrayPool.Return(byteBuffer);
    }


    /// <summary>
    /// Sends a disconnect message to all clients, and optionally immediately iterates outgoing packets to ensure they are sent.
    /// </summary>
    private void SendDisconnectPackets(List<NetworkConnection> conns, bool iterate)
    {
        // Send message to each client, authenticated or not.
        foreach (NetworkConnection c in conns)
        {
            // Write the packet.
            byte[] byteBuffer =
            {
                (byte)InternalPacketType.Disconnect
            };
            ArraySegment<byte> segment = new(byteBuffer, 0, 1);

            // Send the packet.
            _transportManager.SendToClient(Channel.Reliable, segment, c.ClientId);
        }

        if (iterate)
            _transportManager.IterateOutgoing(true);
    }


    /// <summary>
    /// Sends a client connection state change to owner and other clients if applicable.
    /// </summary>
    private void SendClientConnectionChangePacket(bool connected, NetworkConnection conn)
    {
        // Send a message to all authenticated clients with the clientId that just connected.
        // It is important that the just connected client will also get this, so that they can later successfully get a reference to their own connection.
        ClientConnectionChangeNetMessage changeNetMsg = new(conn.ClientId, connected);

        foreach (NetworkConnection c in Clients.Values.Where(c => c.IsAuthenticated))
            SendMessageToClient(c, changeNetMsg);

        // If this was a new connection, the new client must also receive all currently connected client ids.
        if (!connected)
            return;

        // Send already connected clients to the connection that just joined.
        List<ushort> clientIds = new();
        foreach (int id in Clients.Keys)
        {
            if (id is < 0 or > ushort.MaxValue)
            {
                Logger.Warn($"Client id {id} is out of range for ushort. Ignoring.");
                continue;
            }
            clientIds.Add((ushort)id);
        }
        ConnectedClientsNetMessage allIdsNetMessage = new(clientIds);
        SendMessageToClient(conn, allIdsNetMessage);
    }
}
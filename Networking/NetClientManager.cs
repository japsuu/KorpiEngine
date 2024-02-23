using Common.Logging;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.HighLevel.EventArgs;
using Korpi.Networking.HighLevel.Messages;
using Korpi.Networking.HighLevel.Messages.Handlers;
using Korpi.Networking.LowLevel;
using Korpi.Networking.LowLevel.NetStack.Serialization;
using Korpi.Networking.LowLevel.Transports.EventArgs;
using Korpi.Networking.Utility;

namespace Korpi.Networking;

/// <summary>
/// Manages the network client.
/// Does not deal with game logic, only with network communication.
/// </summary>
public class NetClientManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetClientManager));
    private readonly Dictionary<ushort, MessageHandlerCollection> _messageHandlers = new();
    private readonly NetworkManager _netManager;
    private readonly TransportManager _transportManager;

    /// <summary>
    /// NetworkConnection of the local client.
    /// </summary>
    public NetworkConnection? Connection;

    /// <summary>
    /// True if the local client is connected to the server.
    /// </summary>
    public bool Started { get; private set; }

    /// <summary>
    /// All currently connected clients (peers) by clientId.
    /// </summary>
    public readonly Dictionary<int, NetworkConnection> Clients = new();

    /// <summary>
    /// Called after local client has authenticated (when the client receives a welcome message from the server).
    /// </summary>
    public event Action? Authenticated;

    /// <summary>
    /// Called after the local client connection state changes.
    /// </summary>  //BUG: Do not use the low-level args, create a high-level args class instead.
    public event Action<ClientConnectionStateArgs>? ClientConnectionStateChanged;

    /// <summary>
    /// Called when a client other than self connects.
    /// </summary>  //BUG: Do not use the low-level args, create a high-level args class instead.
    public event Action<RemoteConnectionStateArgs>? RemoteConnectionStateChanged;

    /// <summary>
    /// Called when we receive a list of all connected clients from the server (usually right after connecting).
    /// </summary>  //BUG: Do not use the low-level args, create a high-level args class instead.
    public event Action<ClientListArgs>? ReceivedConnectedClientsList;


    /// <summary>
    /// Creates a new client manager using the specified transport.
    /// </summary>
    /// <param name="netManager">The network manager owning this client manager.</param>
    /// <param name="transportManager">The transport to use.</param>
    public NetClientManager(NetworkManager netManager, TransportManager transportManager)
    {
        _netManager = netManager;
        _transportManager = transportManager;
        _transportManager.Transport.LocalClientReceivedPacket += OnClientReceivePacket;
        _transportManager.Transport.LocalClientConnectionStateChanged += OnLocalClientConnectionStateChanged;

        // Listen for other clients connections from server.
        RegisterMessageHandler<ClientConnectionChangeNetMessage>(OnReceiveClientConnectionPacket);
        RegisterMessageHandler<ConnectedClientsNetMessage>(OnReceiveConnectedClientsMessage);
    }


    /// <summary>
    /// Called when a new client connects or disconnects.
    /// </summary>
    /// <param name="message">The message containing the connection change information.</param>
    /// <param name="channel">The channel the message was received on.</param>
    private void OnReceiveClientConnectionPacket(ClientConnectionChangeNetMessage message, Channel channel)
    {
        bool isNewConnection = message.Connected;
        int clientId = message.ClientId;
        RemoteConnectionStateArgs rcs = new(isNewConnection ? RemoteConnectionState.Started : RemoteConnectionState.Stopped, clientId);

        // If a new connection, invoke event after adding conn to clients, otherwise invoke event before conn is removed from clients.
        if (isNewConnection)
        {
            Clients[clientId] = new NetworkConnection(_netManager, clientId, false);
            RemoteConnectionStateChanged?.Invoke(rcs);
        }
        else
        {
            RemoteConnectionStateChanged?.Invoke(rcs);
            if (!Clients.TryGetValue(clientId, out NetworkConnection? c))
                return;

            c.Dispose();
            Clients.Remove(clientId);
        }
    }


    /// <summary>
    /// Called when the server sends a list of all connected clients to the client.
    /// </summary>
    /// <param name="message">The message containing the list of connected clients.</param>
    /// <param name="channel">The channel the message was received on.</param>
    private void OnReceiveConnectedClientsMessage(ConnectedClientsNetMessage message, Channel channel)
    {
        NetworkManager.ClearClientsCollection(Clients);

        List<ushort> collection = message.ClientIds;
        // Create NetworkConnection objects for connected clients.
        int count = collection.Count;
        for (int i = 0; i < count; i++)
        {
            int id = collection[i];
            Clients[id] = new NetworkConnection(_netManager, id, false);
        }

        ReceivedConnectedClientsList?.Invoke(new ClientListArgs(collection));
    }


    /// <summary>
    /// Called when the client receives a message from the server.
    /// </summary>
    /// <param name="args">The message and channel received.</param>
    private void OnClientReceivePacket(ClientReceivedDataArgs args)
    {
        Logger.Verbose($"Received segment {args.Segment.AsStringHex()} from server.");
        if (args.Segment.Array == null)
        {
            Logger.Warn("Received a packet with null data.");
            return;
        }
        
        BitBuffer buffer = BufferPool.GetBitBuffer();
        buffer.FromArray(args.Segment.Array, args.Segment.Count);
        InternalPacketType packetType = (InternalPacketType)buffer.ReadByte();
        
        switch (packetType)
        {
            case InternalPacketType.Unset:
                Logger.Warn("Received a packet with an unset type.");
                break;
            case InternalPacketType.Welcome:
                ParseWelcomePacket(buffer);
                break;
            case InternalPacketType.Message:
                ParseMessagePacket(buffer, args.Channel);
                break;
            case InternalPacketType.Disconnect:
                Disconnect();
                break;
            default:
                Logger.Warn($"Received a message with an unknown packet type {packetType}.");
                break;
        }
    }


    private void ParseMessagePacket(BitBuffer buffer, Channel channel)
    {
        ushort messageId = buffer.ReadUShort();
        NetMessage netMessage = MessageManager.MessageTypeCache.CreateInstance(messageId);
        netMessage.Deserialize(buffer);

        if (!_messageHandlers.TryGetValue(messageId, out MessageHandlerCollection? packetHandler))
        {
            Logger.Warn($"Received a {netMessage} but no handler is registered for it. Ignoring.");
            return;
        }
        
        Logger.Debug($"Received message {netMessage} from server.");

        packetHandler.InvokeHandlers(netMessage, channel);
    }


    private void ParseWelcomePacket(BitBuffer buffer)
    {
        // The ClientConnectionChangeMessage and ConnectedClientsMessage should have already been received, so we can assume Clients contains this client too.
        ushort clientId = buffer.ReadUShort();
        if (!Clients.TryGetValue(clientId, out Connection))
        {
            // This should never happen unless the connection is dropping and the ClientConnectionChangeMessage is lost (or arrives late).
            Logger.Warn(
                "Local client connection could not be found while receiving the Welcome message." +
                "This can occur if the client is receiving a message immediately before losing connection.");
            Connection = new NetworkConnection(_netManager, clientId, false);
        }

        Logger.Info($"Received welcome message from server. Assigned clientId is {clientId}.");

        // Mark local connection as authenticated.
        Connection.SetAuthenticated();
        Authenticated?.Invoke();
    }


    /// <summary>
    /// Called when the local client connection state changes.
    /// </summary>
    /// <param name="args">The new connection state.</param>
    private void OnLocalClientConnectionStateChanged(ClientConnectionStateArgs args)
    {
        LocalConnectionState state = args.ConnectionState;
        Started = state == LocalConnectionState.Started;

        if (!Started)
        {
            Connection = null;
            NetworkManager.ClearClientsCollection(Clients);
        }

        string tName = _transportManager.TransportTypeName;
        string socketInformation = string.Empty;
        if (state == LocalConnectionState.Starting)
            socketInformation = $" Server IP is {_transportManager.GetClientAddress()}, port is {_transportManager.GetPort()}.";
        Logger.Info($"Local client is {state.ToString().ToLower()} for {tName}.{socketInformation}");

        ClientConnectionStateChanged?.Invoke(args);
    }


    /// <summary>
    /// Connects to the server at the specified address and port.
    /// </summary>
    /// <param name="address">The address of the server.</param>
    /// <param name="port">The port of the server.</param>
    public void Connect(string address, ushort port)
    {
        _transportManager.SetClientAddress(address);
        _transportManager.SetPort(port);
        _transportManager.StartConnection(false);
    }


    /// <summary>
    /// Disconnects from the currently connected server.
    /// </summary>
    public void Disconnect()
    {
        _transportManager.StopConnection(false);
    }


    /// <summary>
    /// Registers a method to call when a message of the specified type arrives.
    /// </summary>
    /// <param name="handler">Method to call.</param>
    /// <typeparam name="T"></typeparam>
    public void RegisterMessageHandler<T>(Action<T, Channel> handler) where T : NetMessage
    {
        ushort key = MessageManager.MessageIdCache.GetId<T>();

        if (!_messageHandlers.TryGetValue(key, out MessageHandlerCollection? packetHandler))
        {
            packetHandler = new ServerMessageHandler<T>();
            _messageHandlers.Add(key, packetHandler);
        }

        packetHandler.RegisterHandler(handler);
    }


    /// <summary>
    /// Unregisters a method from being called when a message of the specified type arrives.
    /// </summary>
    /// <param name="handler">The method to unregister.</param>
    /// <typeparam name="T">Type of message to unregister.</typeparam>
    public void UnregisterMessageHandler<T>(Action<T, Channel> handler) where T : NetMessage
    {
        ushort key = MessageManager.MessageIdCache.GetId<T>();
        if (_messageHandlers.TryGetValue(key, out MessageHandlerCollection? messageHandler))
            messageHandler.UnregisterHandler(handler);
    }


    /// <summary>
    /// Sends a message to a connection.
    /// </summary>
    /// <typeparam name="T">Type of message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="channel">Channel to send on.</param>
    public void SendMessageToServer<T>(T message, Channel channel = Channel.Reliable) where T : NetMessage
    {
        if (!Started)
        {
            Logger.Error($"Local connection is not started, cannot send message of type {message}.");
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
        
        Logger.Debug($"Sending message {message} to server.");

        // Send the packet.
        _transportManager.SendToServer(channel, segment);
        ByteArrayPool.Return(byteBuffer);
    }
}
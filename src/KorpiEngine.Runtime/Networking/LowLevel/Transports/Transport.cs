using KorpiEngine.Core.Logging;
using KorpiEngine.Networking.HighLevel;
using KorpiEngine.Networking.HighLevel.Connections;
using KorpiEngine.Networking.LowLevel.Transports.EventArgs;

namespace KorpiEngine.Networking.LowLevel.Transports;

/// <summary>
/// Represents an object that can handle network messages.
/// </summary>
public abstract class Transport : IDisposable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Transport));
    private const string NOT_SUPPORTED_MESSAGE = "The current transport does not support the feature '{0}'.";

    /// <summary>
    /// NetworkManager for this transport.
    /// </summary>
    public NetworkManager NetworkManager { get; private set; } = null!;


    public virtual void Initialize(NetworkManager networkManager)
    {
        NetworkManager = networkManager;
    }


    #region State Management

    /// <summary>
    /// Called when a connection state changes for the local client.
    /// </summary>
    public abstract event Action<ClientConnectionStateArgs>? LocalClientConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for the local server.
    /// </summary>
    public abstract event Action<ServerConnectionStateArgs>? LocalServerConnectionStateChanged;

    /// <summary>
    /// Called when a connection state changes for a remote client.
    /// </summary>
    public abstract event Action<RemoteConnectionStateArgs>? RemoteClientConnectionStateChanged;


    /// <summary>
    /// Gets the current local ConnectionState.
    /// </summary>
    /// <param name="asServer">True if getting ConnectionState for the server.</param>
    public abstract LocalConnectionState GetLocalConnectionState(bool asServer);


    /// <summary>
    /// Gets the current ConnectionState of a client connected to the server. Can only be called on the server.
    /// </summary>
    /// <param name="connectionId">ConnectionId to get ConnectionState for.</param>
    public abstract RemoteConnectionState GetRemoteConnectionState(int connectionId);
    

    /// <summary>
    /// Gets the address of a remote connection Id.
    /// </summary>
    /// <param name="connectionId">Connection id to get the address for.</param>
    /// <returns></returns>
    public abstract string GetRemoteConnectionAddress(int connectionId);

    #endregion

    #region Sending Data

    /// <summary>
    /// Sends to the server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="segment"></param>
    public abstract void SendToServer(Channel channel, ArraySegment<byte> segment);


    /// <summary>
    /// Sends to a client.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="segment"></param>
    /// <param name="connectionId">ConnectionId to send to.</param>
    public abstract void SendToClient(Channel channel, ArraySegment<byte> segment, int connectionId);

    #endregion

    #region Receiving Data

    /// <summary>
    /// Called when the client receives data.
    /// </summary>
    public abstract event Action<ClientReceivedDataArgs>? LocalClientReceivedPacket;


    /// <summary>
    /// Called when the server receives data.
    /// </summary>
    public abstract event Action<ServerReceivedDataArgs>? LocalServerReceivedPacket;

    #endregion

    #region Iteration and Updating
    
    /// <summary>
    /// Polls the sockets for incoming data.
    /// </summary>
    public abstract void PollSockets();

    
    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="asServer">True to process data received on the server.</param>
    public abstract void IterateIncomingData(bool asServer);


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="asServer">True to process data received on the server.</param>
    public abstract void IterateOutgoingData(bool asServer);

    #endregion

    #region Configuration

    /// <summary>
    /// Gets how long in milliseconds until either the server or client socket must go without data before being timed out.
    /// If the transport does not support this method the value -1 is returned.
    /// </summary>
    /// <param name="asServer">True to get the timeout for the server socket, false for the client socket.</param>
    /// <returns></returns>
    public virtual int GetTimeout(bool asServer)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetTimeout));
        return -1;
    }


    /// <summary>
    /// Returns the maximum number of clients allowed to connect to the server.
    /// If the transport does not support this method the value -1 is returned.
    /// </summary>
    /// <returns>Maximum clients transport allows.</returns>
    public virtual int GetMaximumClients()
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetMaximumClients));
        return -1;
    }


    /// <summary>
    /// Sets the maximum number of clients allowed to connect to the server.
    /// If applied at runtime and clients exceed this value existing clients will stay connected but new clients may not connect.
    /// </summary>
    /// <param name="value">Maximum clients to allow.</param>
    public virtual void SetMaximumClients(int value)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetMaximumClients));
    }


    /// <summary>
    /// Sets which address the client will connect to.
    /// </summary>
    /// <param name="address">Address client will connect to.</param>
    public virtual void SetClientConnectAddress(string address)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetClientConnectAddress));
    }


    /// <summary>
    /// Returns which address the client will connect to.
    /// </summary>
    public virtual string GetClientConnectAddress()
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetClientConnectAddress));
        return string.Empty;
    }


    /// <summary>
    /// Sets which address the server will bind to.
    /// </summary>
    /// <param name="type">The type of address to bind to.</param>
    /// <param name="address">Address server will bind to.</param>
    public virtual void SetServerBindAddress(AddressType type, string address)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetServerBindAddress));
    }


    /// <summary>
    /// Gets which address the server will bind to.
    /// </summary>
    public virtual string GetServerBindAddress(AddressType type)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetServerBindAddress));
        return string.Empty;
    }


    /// <summary>
    /// Sets which port to use.
    /// </summary>
    /// <param name="port">Port to use.</param>
    public virtual void SetPort(ushort port)
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(SetPort));
    }


    /// <summary>
    /// Gets which port to use.
    /// </summary>
    public virtual ushort GetPort()
    {
        Logger.WarnFormat(NOT_SUPPORTED_MESSAGE, nameof(GetPort));
        return 0;
    }

    #endregion

    #region Starting and stopping

    /// <summary>
    /// Starts the local server or client using configured settings.
    /// </summary>
    /// <param name="server">True to start server.</param>
    public abstract bool StartLocalConnection(bool server);


    /// <summary>
    /// Stops the local server or client.
    /// </summary>
    /// <param name="server">True to stop server.</param>
    public abstract bool StopLocalConnection(bool server);


    /// <summary>
    /// Stops a remote client from the server, disconnecting the client.
    /// </summary>
    /// <param name="connectionId">ConnectionId of the client to disconnect.</param>
    /// <param name="immediate">True to disconnect immediately.</param>
    public abstract bool StopRemoteConnection(int connectionId, bool immediate);


    /// <summary>
    /// Stops both client and server.
    /// </summary>
    public abstract void Shutdown();

    #endregion


    /// <summary>
    /// Gets the MTU for the unreliable channel, in bytes.
    /// This should take header size into consideration.
    /// For example, if MTU is 1200 bytes and a packet header for the unreliable channel is 10 bytes, this method should return 1190.
    /// </summary>
    public abstract int GetUnreliableMTU();


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO release managed resources here
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
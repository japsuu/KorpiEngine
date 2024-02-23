using KorpiEngine.Networking.HighLevel.Connections;

namespace KorpiEngine.Networking.LowLevel.Transports.EventArgs;

public readonly struct RemoteConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public readonly RemoteConnectionState ConnectionState;
    
    /// <summary>
    /// ConnectionId for which client the state changed. Will be 0 if <see cref="ConnectionState"/> was for the server.
    /// </summary>
    public readonly int ConnectionId;


    public RemoteConnectionStateArgs(RemoteConnectionState state, int connectionEventConnectionId)
    {
        ConnectionState = state;
        ConnectionId = connectionEventConnectionId;
    }
}
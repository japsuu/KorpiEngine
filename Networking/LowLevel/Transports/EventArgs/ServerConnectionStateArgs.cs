using KorpiEngine.Networking.HighLevel.Connections;

namespace KorpiEngine.Networking.LowLevel.Transports.EventArgs;

public readonly struct ServerConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public readonly LocalConnectionState ConnectionState;


    public ServerConnectionStateArgs(LocalConnectionState connectionState)
    {
        ConnectionState = connectionState;
    }
}
using KorpiEngine.Multiplayer.HighLevel.Connections;

namespace KorpiEngine.Multiplayer.LowLevel.Transports.EventArgs;

public readonly struct ClientConnectionStateArgs
{
    /// <summary>
    /// New connection state.
    /// </summary>
    public readonly LocalConnectionState ConnectionState;


    public ClientConnectionStateArgs(LocalConnectionState connectionState)
    {
        ConnectionState = connectionState;
    }
}
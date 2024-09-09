using KorpiEngine.Networking.Multiplayer.HighLevel.Connections;

namespace KorpiEngine.Networking.Multiplayer.LowLevel;

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
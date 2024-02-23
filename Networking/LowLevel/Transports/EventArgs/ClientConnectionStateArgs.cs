using Korpi.Networking.HighLevel.Connections;

namespace Korpi.Networking.LowLevel.Transports.EventArgs;

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
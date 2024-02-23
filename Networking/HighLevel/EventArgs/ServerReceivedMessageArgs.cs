using Korpi.Networking.HighLevel.Connections;
using Korpi.Networking.HighLevel.Messages;

namespace Korpi.Networking.HighLevel.EventArgs;

/// <summary>
/// Container about a message received on the server.
/// </summary>
public readonly struct ServerReceivedMessageArgs
{
    /// <summary>
    /// The message received.
    /// </summary>
    public readonly NetMessage Message;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;

    /// <summary>
    /// Connection of client which sent the message.
    /// </summary>
    public readonly NetworkConnection Connection;


    public ServerReceivedMessageArgs(NetMessage message, Channel channel, NetworkConnection connection)
    {
        Message = message;
        Channel = channel;
        Connection = connection;
    }
}
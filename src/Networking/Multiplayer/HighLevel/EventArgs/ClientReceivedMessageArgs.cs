using KorpiEngine.Multiplayer.HighLevel.Messages;

namespace KorpiEngine.Multiplayer.HighLevel.EventArgs;

/// <summary>
/// Container about a message received on the client, from the server.
/// </summary>
public readonly struct ClientReceivedMessageArgs
{
    /// <summary>
    /// The message received.
    /// </summary>
    public readonly NetMessage Message;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;


    public ClientReceivedMessageArgs(NetMessage message, Channel channel)
    {
        Message = message;
        Channel = channel;
    }
}
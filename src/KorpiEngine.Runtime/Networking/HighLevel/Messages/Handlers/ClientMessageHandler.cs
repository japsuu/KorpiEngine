using KorpiEngine.Networking.HighLevel.Connections;

namespace KorpiEngine.Networking.HighLevel.Messages.Handlers;

/// <summary>
/// Handles packets received on server, from clients.
/// </summary>
internal class ClientMessageHandler<T> : MessageHandlerCollection
{
    private readonly List<Action<NetworkConnection, T, Channel>> _handlers = new();
    public override bool RequireAuthentication { get; }


    public ClientMessageHandler(bool requireAuthentication)
    {
        RequireAuthentication = requireAuthentication;
    }


    public override void RegisterHandler(object obj)
    {
        if (obj is Action<NetworkConnection, T, Channel> handler)
            _handlers.Add(handler);
    }


    public override void UnregisterHandler(object obj)
    {
        if (obj is Action<NetworkConnection, T, Channel> handler)
            _handlers.Remove(handler);
    }


    public override void InvokeHandlers(NetworkConnection conn, NetMessage netMessage, Channel channel)
    {
        if (netMessage is not T tPacket)
            return;

        foreach (Action<NetworkConnection, T, Channel> handler in _handlers)
            handler.Invoke(conn, tPacket, channel);
    }


    public override void InvokeHandlers(NetMessage netMessage, Channel channel)
    {
        // Server does not handle packets from the server.
        throw new NotImplementedException();
    }
}
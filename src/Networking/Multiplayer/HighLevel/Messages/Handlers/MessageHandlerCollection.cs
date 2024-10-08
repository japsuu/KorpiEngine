﻿using KorpiEngine.Networking.Multiplayer.HighLevel.Connections;

namespace KorpiEngine.Networking.Multiplayer.HighLevel.Messages;

internal abstract class MessageHandlerCollection
{
    public abstract void RegisterHandler(object obj);
    public abstract void UnregisterHandler(object obj);
    public abstract void InvokeHandlers(NetMessage netMessage, Channel channel);
    public abstract void InvokeHandlers(NetworkConnection conn, NetMessage netMessage, Channel channel);
    public abstract bool RequireAuthentication { get; }
}
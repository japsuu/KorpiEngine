﻿using KorpiEngine.Networking.Multiplayer.HighLevel.Connections;

namespace KorpiEngine.Networking.Multiplayer.LowLevel.Transports.EventArgs;

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
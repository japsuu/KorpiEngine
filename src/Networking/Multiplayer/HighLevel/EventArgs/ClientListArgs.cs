﻿namespace KorpiEngine.Networking.Multiplayer.HighLevel.EventArgs;

public readonly struct ClientListArgs
{
    public readonly List<ushort> ClientIds;
    
    
    public ClientListArgs(List<ushort> clientIds)
    {
        ClientIds = clientIds;
    }
}
﻿using KorpiEngine.Networking.Multiplayer.HighLevel;

namespace KorpiEngine.Networking.Multiplayer.LowLevel;

/// <summary>
/// Container about data received on the local client.
/// </summary>
public readonly struct ClientReceivedDataArgs
{
    /// <summary>
    /// Data received.
    /// </summary>
    public readonly ArraySegment<byte> Segment;

    /// <summary>
    /// Channel data was received on.
    /// </summary>
    public readonly Channel Channel;


    public ClientReceivedDataArgs(ArraySegment<byte> segment, Channel channel)
    {
        Segment = segment;
        Channel = channel;
    }
}
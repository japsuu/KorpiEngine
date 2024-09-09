﻿using System.Net;

namespace KorpiEngine.Networking.Multiplayer.LowLevel
{
    public abstract class PacketLayerBase
    {
        public readonly int ExtraPacketSizeForLayer;

        protected PacketLayerBase(int extraPacketSizeForLayer)
        {
            ExtraPacketSizeForLayer = extraPacketSizeForLayer;
        }

        public abstract void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length);
        public abstract void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);
    }
}

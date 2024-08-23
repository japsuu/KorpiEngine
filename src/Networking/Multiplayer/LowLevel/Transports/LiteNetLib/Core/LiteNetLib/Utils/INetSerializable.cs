namespace KorpiEngine.Networking.Multiplayer.LowLevel.Transports.LiteNetLib.Core.LiteNetLib.Utils
{
    public interface INetSerializable
    {
        void Serialize(NetDataWriter writer);
        void Deserialize(NetDataReader reader);
    }
}

namespace KorpiEngine.Networking.Multiplayer.LowLevel
{
    public interface INetSerializable
    {
        void Serialize(NetDataWriter writer);
        void Deserialize(NetDataReader reader);
    }
}

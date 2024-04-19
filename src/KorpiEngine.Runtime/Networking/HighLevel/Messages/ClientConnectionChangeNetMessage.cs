using KorpiEngine.Networking.LowLevel.NetStack.Serialization;

namespace KorpiEngine.Networking.HighLevel.Messages;

/// <summary>
/// Packet sent to all clients when a client connects or disconnects.
/// </summary>
public class ClientConnectionChangeNetMessage : NetMessage
{
    public int ClientId { get; private set; }
    public bool Connected { get; private set; }


    public ClientConnectionChangeNetMessage(int clientId, bool connected)
    {
        ClientId = clientId;
        Connected = connected;
    }


    protected override void SerializeInternal(BitBuffer buffer)
    {
        buffer.AddInt(ClientId);
        buffer.AddBool(Connected);
    }


    protected override void DeserializeInternal(BitBuffer buffer)
    {
        ClientId = buffer.ReadInt();
        Connected = buffer.ReadBool();
    }
}
using KorpiEngine.Networking.LowLevel.NetStack.Serialization;

namespace KorpiEngine.Networking.HighLevel.Messages;

/// <summary>
/// Packet sent to a new client when they connect to the server.
/// </summary>
public class ConnectedClientsNetMessage : NetMessage
{
    public List<ushort> ClientIds { get; private set; }
    
    
    public ConnectedClientsNetMessage(List<ushort> clientIds)
    {
        ClientIds = clientIds;
    }


    protected override void SerializeInternal(BitBuffer buffer)
    {
        buffer.AddUShort((ushort)ClientIds.Count);
        foreach (ushort clientId in ClientIds)
            buffer.AddUShort(clientId);
    }


    protected override void DeserializeInternal(BitBuffer buffer)
    {
        ushort count = buffer.ReadUShort();
        ClientIds = new List<ushort>(count);
        for (int i = 0; i < count; i++)
            ClientIds.Add(buffer.ReadUShort());
    }
}
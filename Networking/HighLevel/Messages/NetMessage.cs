using KorpiEngine.Networking.LowLevel.NetStack.Serialization;

namespace KorpiEngine.Networking.HighLevel.Messages;

/// <summary>
/// Represents an message (packet) that can be sent over the network.<br/>
/// You can safely cache instances of this class, as it is immutable and won't be reused.<br/><br/>
/// Any constructors will be skipped when deserializing, so don't rely on them being called.
/// </summary>
public abstract class NetMessage
{
    /// <summary>
    /// Serializes this message into a bit buffer.
    /// </summary>
    /// <param name="buffer">Empty buffer to serialize the message into.</param>
    public void Serialize(BitBuffer buffer)
    {
        ushort id = MessageManager.MessageIdCache.GetId(GetType());
        buffer.AddUShort(id);
        SerializeInternal(buffer);
    }

    
    /// <summary>
    /// Deserializes this message from a bit buffer.
    /// </summary>
    /// <param name="buffer">Buffer containing the serialized message.</param>
    public void Deserialize(BitBuffer buffer)
    {
        DeserializeInternal(buffer);
    }


    protected abstract void SerializeInternal(BitBuffer buffer);
    protected abstract void DeserializeInternal(BitBuffer buffer);


    public override string ToString()
    {
        return $"{GetType().Name} (ID: {MessageManager.MessageIdCache.GetId(GetType())})";
    }
}
using KorpiEngine.Networking.LowLevel.NetStack.Serialization;

namespace KorpiEngine.Networking.HighLevel.Messages;

/// <summary>
/// Sent by the server to the client to indicate the result of the authentication process.
/// </summary>
public class AuthResponseNetMessage : NetMessage
{
    public bool Success { get; private set; }
    public string Reason { get; private set; }


    public AuthResponseNetMessage(bool success, string? reason)
    {
        Success = success;
        Reason = reason ?? string.Empty;
    }


    protected override void SerializeInternal(BitBuffer buffer)
    {
        buffer.AddBool(Success);
        buffer.AddString(Reason);
    }


    protected override void DeserializeInternal(BitBuffer buffer)
    {
        Success = buffer.ReadBool();
        Reason = buffer.ReadString();
    }
}
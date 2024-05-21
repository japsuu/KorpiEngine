using KorpiEngine.Networking.LowLevel.NetStack.Serialization;

namespace KorpiEngine.Networking.HighLevel.Messages;

/// <summary>
/// Sent by the client to the server to authenticate with a password.
/// </summary>
public class AuthPasswordNetMessage : NetMessage
{
    public string Username { get; private set; }
    public string Password { get; private set; }


    /// <summary>
    /// Constructs a new AuthPasswordNetMessage.
    /// </summary>
    /// <param name="username">Username to authenticate with. Has a 255 character limit.</param>
    /// <param name="password">Password to authenticate with. Has a 255 character limit.</param>
    public AuthPasswordNetMessage(string username, string password)
    {
        Username = username;
        Password = password;
    }


    protected override void SerializeInternal(BitBuffer buffer)
    {
        buffer.AddString(Username);
        buffer.AddString(Password);
    }


    protected override void DeserializeInternal(BitBuffer buffer)
    {
        Username = buffer.ReadString();
        Password = buffer.ReadString();
    }
}
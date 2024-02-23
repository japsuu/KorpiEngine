namespace Korpi.Networking.LowLevel;

/// <summary>
/// Represents the type of data being sent or received.
/// </summary>
internal enum InternalPacketType : byte
{
    /// <summary>
    /// Packet contains unknown data.
    /// </summary>
    Unset = 0,
    
    /// <summary>
    /// Welcome packet.
    /// Contains the client's connectionId.
    /// </summary>
    Welcome = 1,
    
    /// <summary>
    /// Packet contains a message.
    /// </summary>
    Message = 2,
    
    /// <summary>
    /// Disconnect packet.
    /// The client should immediately disconnect from the server.
    /// </summary>
    Disconnect = 3,
}
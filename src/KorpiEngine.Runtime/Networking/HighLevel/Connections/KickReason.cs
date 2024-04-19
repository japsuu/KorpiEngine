namespace KorpiEngine.Networking.HighLevel.Connections;

public enum KickReason : short
{
    /// <summary>
    /// No reason was specified.
    /// </summary>
    Unset = 0,
    
    /// <summary>
    /// Client performed an action which could only be done if trying to exploit the server.
    /// </summary>
    ExploitAttempt = 1,
    
    /// <summary>
    /// Data received from the client could not be parsed. This rarely indicates an attack.
    /// </summary>
    MalformedData = 2,
    
    /// <summary>
    /// There was a problem with the server that required the client to be kicked.
    /// </summary>
    UnexpectedProblem = 3,
}
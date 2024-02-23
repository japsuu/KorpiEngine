namespace Korpi.Networking.HighLevel.Connections;

/// <summary>
/// States a remote client can be in.
/// </summary>
public enum RemoteConnectionState : byte
{
    /// <summary>
    /// Connection is fully stopped.
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Connection is established.
    /// </summary>
    Started = 2
}
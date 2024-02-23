using Common.Logging;

namespace Korpi.Networking.HighLevel.Connections;

/// <summary>
/// A container for a connected client used to perform actions on and gather information for the declared client.
/// </summary>
public class NetworkConnection : IDisposable, IEquatable<NetworkConnection>
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(NetworkConnection));

    private readonly NetworkManager _netManager;

    /// <summary>
    /// True if this connection is authenticated. Only available to server.
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// True if this connection is valid and not Disconnecting.
    /// </summary>
    public bool IsActive => IsValid && !Disconnecting;

    /// <summary>
    /// True if this connection is valid. An invalid connection indicates no client is set for this reference.
    /// </summary>
    public bool IsValid => ClientId >= 0;

    /// <summary>
    /// Unique Id for this connection.
    /// </summary>
    public readonly int ClientId;

    /// <summary>
    /// True if this connection is being disconnected. Only available to server.
    /// </summary>
    public bool Disconnecting { get; private set; }
    
    /// <summary>
    /// Returns if this connection is for the local client.
    /// </summary>
    public bool IsLocalClient => _netManager.Client.Connection == this;

    
    public NetworkConnection(NetworkManager netManager, int clientId, bool asServer)
    {
        _netManager = netManager;
        ClientId = clientId;

        if (asServer)
        {
            
        }
    }


    /// <summary>
    /// Disconnects this connection. Only available on the server.
    /// </summary>
    /// <param name="immediate">True to disconnect immediately, false to first send all pending packets to them.</param>
    public void Disconnect(bool immediate)
    {
        if (!IsValid)
        {
            Logger.Warn("Disconnect called on an invalid connection.");
            return;
        }

        if (Disconnecting)
        {
            Logger.Warn($"ClientId {ClientId} is already disconnecting.");
            return;
        }

        SetDisconnecting(true);

        // TODO: Send out any pending information to the client, then disconnect it.
        _netManager.TransportManager.StopConnection(ClientId, immediate);
    }
    
    
    /// <summary>
    /// Returns the address of this connection.
    /// </summary>
    /// <returns></returns>
    public string GetAddress()
    {
        if (!IsValid)
            return string.Empty;

        return _netManager.TransportManager.GetConnectionAddress(ClientId);
    }


    /// <summary>
    /// Sets connection as authenticated.
    /// </summary>
    internal void SetAuthenticated()
    {
        IsAuthenticated = true;
    }


    /// <summary>
    /// Sets Disconnecting boolean for this connection.
    /// </summary>
    internal void SetDisconnecting(bool value)
    {
        Disconnecting = value;
    }


    public override string ToString()
    {
        return $"Id [{ClientId}] Address [{GetAddress()}]";
    }


    public void Dispose()
    {
        // TODO release managed resources here
    }
    
    
    public bool Equals(NetworkConnection? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ClientId == other.ClientId;
    }


    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((NetworkConnection)obj);
    }


    public override int GetHashCode() => ClientId;
    public static bool operator ==(NetworkConnection? left, NetworkConnection? right) => Equals(left, right);
    public static bool operator !=(NetworkConnection? left, NetworkConnection? right) => !Equals(left, right);
}
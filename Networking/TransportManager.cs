using Common.Logging;
using Korpi.Networking.HighLevel;
using Korpi.Networking.HighLevel.Messages;
using Korpi.Networking.LowLevel.Transports;
using Korpi.Networking.Utility;

namespace Korpi.Networking;

public class TransportManager
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(TransportManager));
    private readonly NetworkManager _netManager;

    public readonly Transport Transport;
    public event Action<bool>? IterateOutgoingStart;
    public event Action<bool>? IterateOutgoingEnd;
    public event Action<bool>? IterateIncomingStart;
    public event Action<bool>? IterateIncomingEnd;
    
    public string TransportTypeName => Transport.GetType().Name;


    public TransportManager(NetworkManager netManager, Transport transport)
    {
        _netManager = netManager;
        Transport = transport;
        MessageManager.RegisterAllMessages();
    }


    public string GetConnectionAddress(int clientId) => Transport.GetRemoteConnectionAddress(clientId);
    public string GetClientAddress() => Transport.GetClientConnectAddress();

    public string GetServerBindAddress(AddressType addressType) => Transport.GetServerBindAddress(addressType);
    public ushort GetPort() => Transport.GetPort();


    public void SetClientAddress(string address)
    {
        Transport.SetClientConnectAddress(address);
    }


    public void SetServerBindAddress(string address)
    {
        Transport.SetServerBindAddress(AddressType.IPV4, address);
    }


    public void SetPort(ushort port)
    {
        Transport.SetPort(port);
    }


    public void SetMaximumClients(int maxConnections)
    {
        Transport.SetMaximumClients(maxConnections);
    }


    public void StartConnection(bool isServer)
    {
        Transport.StartLocalConnection(isServer);
    }


    public void StopConnection(bool isServer)
    {
        Transport.StopLocalConnection(isServer);
    }


    public void StopConnection(int clientId, bool immediate)
    {
        Transport.StopRemoteConnection(clientId, immediate);
    }


    public void SendToClient(Channel channel, ArraySegment<byte> segment, int clientId)
    {
        Logger.Verbose($"Sending segment '{segment.AsStringHex()}' to client {clientId}.");
        Transport.SendToClient(channel, segment, clientId);
    }


    public void SendToServer(Channel channel, ArraySegment<byte> segment)
    {
        Logger.Verbose($"Sending segment '{segment.AsStringHex()}' to server.");
        Transport.SendToServer(channel, segment);
    }
    
    
    /// <summary>
    /// Polls the sockets for incoming data.
    /// </summary>
    internal void PollSockets()
    {
        Transport.PollSockets();
    }


    /// <summary>
    /// Processes data received by the socket.
    /// </summary>
    /// <param name="asServer">True to process data received on the server.</param>
    internal void IterateIncoming(bool asServer)
    {
        IterateIncomingStart?.Invoke(asServer);
        Transport.IterateIncomingData(asServer);
        IterateIncomingEnd?.Invoke(asServer);
    }


    /// <summary>
    /// Processes data to be sent by the socket.
    /// </summary>
    /// <param name="asServer">True to process data to be sent on the server.</param>
    internal void IterateOutgoing(bool asServer)
    {
        IterateOutgoingStart?.Invoke(asServer);
        Transport.IterateOutgoingData(asServer);
        IterateOutgoingEnd?.Invoke(asServer);
    }
}
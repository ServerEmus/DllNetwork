using LiteNetLib;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Listeners;

/// <summary>
/// Represent a <see cref="INetEventListener"/> only for Client connection.
/// </summary>
internal class ClientListener : INetEventListener
{
    /// <summary>
    /// Called when a peer connected.
    /// </summary>
    public static event ConnectedDelegate? OnConnected;

    /// <summary>
    /// Called when a peer disconnected.
    /// </summary>
    public static event DisconnectedDelegate? OnDisconnected;

    /// <summary>
    /// Gets the lazy loaded <see cref="ClientListener"/>.
    /// </summary>
    public static Lazy<ClientListener> Listener => new(() => new());

    /// <inheritdoc/>
    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.Reject();
    }

    /// <inheritdoc/>
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    /// <inheritdoc/>
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    /// <inheritdoc/>
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        NetworkLog.Logger.Debug($"[ClientListener.OnNetworkReceive] Id: {peer.Id}");
        PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveData(peer, channelNumber, deliveryMethod));
    }

    /// <inheritdoc/>
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    /// <inheritdoc/>
    public void OnPeerConnected(NetPeer peer)
    {
        NetworkLog.Logger.Information($"[ClientListener.OnPeerConnected] Id: {peer.Id}");
        OnConnected?.Invoke(peer);
    }

    /// <inheritdoc/>
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        NetworkLog.Logger.Information($"[ClientListener.OnPeerDisconnected] {peer.Id} {disconnectInfo.SocketErrorCode} {disconnectInfo.Reason}");
        OnDisconnected?.Invoke(peer, disconnectInfo);
    }
}

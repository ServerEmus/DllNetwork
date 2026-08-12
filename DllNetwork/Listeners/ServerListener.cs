using DllNetwork.Settings;
using LiteNetLib;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Listeners;

/// <summary>
/// Represent a <see cref="INetEventListener"/> only for Server connection.
/// </summary>
internal class ServerListener : INetEventListener
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
    /// Gets the lazy loaded <see cref="ServerListener"/>.
    /// </summary>
    public static Lazy<ServerListener> Listener => new(() => new());

    /// <inheritdoc/>
    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (request.Data.AvailableBytes == 0)
        {
            request.Reject();
            return;
        }

        string connectionKey = request.Data.GetString();
        string accountId = request.Data.GetString();

        LiteNetPeer? peer;
        if (string.IsNullOrEmpty(NetworkSettings.Instance.Connection.ConnectionKey)
            || (connectionKey == NetworkSettings.Instance.Connection.ConnectionKey))
        {
            peer = request.Accept();
        }
        else
        {
            request.Reject();
            return;
        }

        if (peer == null)
        {
            return;
        }

        NetworkLog.Logger.Information("[ServerListener.OnConnectionRequest] Request accepted! Id: {id} AID: {accountId}", peer.Id, accountId);
        AccountStorage.SetPeerId(accountId, peer.Id);
    }

    /// <inheritdoc/>
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        NetworkLog.Logger.Error("[ServerListener.OnNetworkError] Error: {peer} {socketError}", endPoint, socketError);
    }

    /// <inheritdoc/>
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    /// <inheritdoc/>
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        NetworkLog.Logger.Debug($"[ServerListener.OnNetworkReceive] Id: {peer.Id}");
        PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveData(peer, channelNumber, deliveryMethod));
    }

    /// <inheritdoc/>
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    /// <inheritdoc/>
    public void OnPeerConnected(NetPeer peer)
    {
        OnConnected?.Invoke(peer);
    }

    /// <inheritdoc/>
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (AccountStorage.TryGetFromPeerId(peer.Id, out string accountId))
        {
            AccountStorage.Remove(accountId);
        }

        OnDisconnected?.Invoke(peer, disconnectInfo);
        NetworkLog.Logger.Information("[ServerListener.OnPeerDisconnected] Peer {peer} disconnected! Reason: {Reason} Error: {Error}", peer, disconnectInfo.Reason, disconnectInfo.SocketErrorCode);
    }
}

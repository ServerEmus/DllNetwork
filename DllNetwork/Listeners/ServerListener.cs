using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace DllNetwork.Listeners;

public class ServerListener : INetEventListener
{
    public static event ConnectedDelegate? OnConnected;
    public static event DisconnectedDelegate? OnDisconnected;

    public static Lazy<ServerListener> Listener => new(() => new());

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
            || (connectionKey == NetworkSettings.Instance.Connection.ConnectionKey)
            )
        {
            peer = request.Accept();
        }
        else
        {
            request.Reject();
            return;
        }

        if (peer == null)
            return;

        Log.Information("[ServerListener.OnConnectionRequest] Request accepted! {id} {accountId}", peer.Id, accountId);

        NetPeerStore.SetPeerId(accountId, peer.Id);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Log.Error("[ServerListener.OnNetworkError] Error: {peer} {socketError}", endPoint, socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        Log.Information($"[ServerListener.OnNetworkReceive] {peer.Id}");
        PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveData(peer, channelNumber, deliveryMethod));
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        OnConnected?.Invoke(peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (NetPeerStore.TryGetFromPeerId(peer.Id, out string accountId))
            NetPeerStore.Remove(accountId);

        OnDisconnected?.Invoke(peer, disconnectInfo);

        Log.Information("[ServerListener.OnPeerDisconnected] Peer {peer} disconnected! Reason: {Reason} Error: {Error}", peer, disconnectInfo.Reason, disconnectInfo.SocketErrorCode);
    }
}

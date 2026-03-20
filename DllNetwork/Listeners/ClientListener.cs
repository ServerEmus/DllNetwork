using LiteNetLib;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Listeners;

public class ClientListener : INetEventListener
{
    public static event ConnectedDelegate? OnConnected;
    public static event DisconnectedDelegate? OnDisconnected;

    public static Lazy<ClientListener> Listener => new(() => new());
    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.Reject();
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        Log.Information($"[ClientListener.OnNetworkReceive] {peer.Id}");
        PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveData(peer, channelNumber, deliveryMethod));
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Log.Information($"[ClientListener.OnPeerConnected] {peer.Id}");
        OnConnected?.Invoke(peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Log.Information($"[ClientListener.OnPeerDisconnected] {peer.Id}");
        OnDisconnected?.Invoke(peer, disconnectInfo);
    }
}

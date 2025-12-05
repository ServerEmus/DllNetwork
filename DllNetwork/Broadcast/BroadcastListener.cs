using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Broadcast;

public class BroadcastListener : INetEventListener
{
    public static BroadcastListener Listener { get; } = new();
    public event EventBasedNetListener.OnPeerConnected? PeerConnected;
    public event EventBasedNetListener.OnPeerDisconnected? PeerDisconnected;

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey(Constants.BroadcastKey);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Log.Error("[Broadcast] Network Error: {EndPoint} {Error}", endPoint, socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        Log.Debug("[Broadcast] Receive: {Peer} {Channel} {Delivery}", peer, channelNumber, deliveryMethod);
        try
        {
            PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveUserData()
            {
                Peer = peer,
                Delivery = deliveryMethod,
                Channel = channelNumber
            });
        }
        catch (ParseException ex)
        {
            Log.Error("[Broadcast] Receive Exception: {EX}", ex);
        }

    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Log.Debug("[Broadcast] Receive Unconnected: {EndPoint} {UnconnectedType}", remoteEndPoint, messageType);
        try
        {
            PacketProcessor.Processor.ReadAllPackets(reader, new ReceiveUserData()
            {
                EndPoint = remoteEndPoint,
                UnconnectedMessage = messageType,
            });
        }
        catch (ParseException ex)
        {
            Log.Error("[Broadcast] Unconnected Exception: {EX}", ex);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        PeerConnected?.Invoke(peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        PeerDisconnected?.Invoke(peer, disconnectInfo);
    }
}

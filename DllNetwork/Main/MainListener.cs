using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Main;

public class MainListener : INetEventListener
{
    public static MainListener Listener { get; } = new();
    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (!string.IsNullOrEmpty(NetworkSettings.Instance.Accept.AcceptKey))
        {
            request.AcceptIfKey(NetworkSettings.Instance.Accept.AcceptKey);
            return;
        }

        if (NetworkSettings.Instance.Accept.AcceptAll)
        {
            request.Accept();
            return;
        }

        if (!string.IsNullOrEmpty(NetworkSettings.Instance.Accept.RejectHexString))
        {
            request.Reject();
            return;
        }

        request.Reject(Convert.FromHexString(NetworkSettings.Instance.Accept.RejectHexString));
    }


    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Log.Error("[Main] Network Error: {EndPoint} {Error}", endPoint, socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
       // Log.Debug("Latency Update: {Peer} {Latency}", peer, latency);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        Log.Debug("[Main] Receive: {Peer} {Channel} {Delivery}", peer, channelNumber, deliveryMethod);
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
            Log.Error("[Main] Receive Exception: {EX}", ex);
        }

    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Log.Debug("[Main] Receive Unconnected: {EndPoint} {UnconnectedType}", remoteEndPoint, messageType);
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
            Log.Error("[Main] Unconnected Exception: {EX}", ex);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Log.Debug("[Main] {LocalPort} connected to: {peer} [{Details}]", peer.NetManager.LocalPort, peer, peer.Details);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Log.Debug("[Main] {LocalPort} disconnected from {peer} [{Details}] with reason: {Reason}", peer.NetManager.LocalPort, peer, peer.Details, disconnectInfo.Reason);
    }
}

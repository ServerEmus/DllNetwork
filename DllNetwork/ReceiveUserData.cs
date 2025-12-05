using LiteNetLib;
using System.Net;

namespace DllNetwork;

public struct ReceiveUserData
{
    public NetPeer Peer;
    public byte Channel;
    public DeliveryMethod Delivery;

    public IPEndPoint EndPoint;
    public UnconnectedMessageType UnconnectedMessage;

    public readonly override string ToString()
    {
        if (Peer == null)
        {
            return $"EP: {EndPoint} UM:{UnconnectedMessage}";
        }
        return $"C: {Channel} D:{Delivery} P:{Peer} [{Peer.Details}]";
    }
}

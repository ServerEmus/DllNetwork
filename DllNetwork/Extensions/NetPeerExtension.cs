using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;

namespace DllNetwork.Extensions;

public static class NetPeerExtension
{

    private readonly static NetDataWriter Writer = new();

    extension(NetPeer peer)
    {
        public string Details
        {
            get 
            {
                return $"IpPort: {peer} PeerId: {peer.Id} RemoteId: {peer.RemoteId}";
            }
        }

        public void SendSerializable<T>(ref T serializable, byte channel = 0, DeliveryMethod delivery = DeliveryMethod.ReliableOrdered) where T : INetSerializable
        {
            Writer.Reset();
            PacketProcessor.Processor.WriteNetSerializable(Writer, ref serializable);
            peer.Send(Writer, channel, delivery);
            Log.Debug("Sent {packet} to Peer: {peer} [{Details}]", serializable, peer, peer.Details);
        }
    }
}

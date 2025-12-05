using DllNetwork.Broadcast;
using LiteNetLib.Utils;
using Serilog;

namespace DllNetwork.PacketWorker;

public static partial class Workers
{
    public static void DiscoveryRequest(DiscoveryRequestPacket packet, ReceiveUserData data)
    {
        // Not a broadcast we dont care about it.
        if (data.UnconnectedMessage is not LiteNetLib.UnconnectedMessageType.Broadcast)
            return;

        // Broadcast not running, we dont care about the discovery (MAIN are not the target audience.)
        if (!BroadcastManager.Instance.IsRunning)
            return;

        Log.Debug("Packet: {packet}, rc: {data}", packet, data);
        PeerAccount.TryAdd(packet.AccountId, data.EndPoint);

        DiscoveryResponsePacket discoveryResponsePacket = new();
        NetDataWriter writer = new();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref discoveryResponsePacket);
        BroadcastManager.Instance.SendUnconnectedMessage(writer, data.EndPoint);
    }
}

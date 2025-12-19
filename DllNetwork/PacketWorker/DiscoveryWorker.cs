using DllNetwork.Broadcast;
using DllNetwork.Main;
using LiteNetLib.Utils;
using Serilog;

namespace DllNetwork.PacketWorker;

public static partial class Workers
{
    public static void Discovery(DiscoveryPacket packet, ReceiveUserData data)
    {
        if (packet.Version < Constants.MinSupportedVersion)
        {
            Log.Warning("Incompatible version {version} from {accountId}. Last supported: {MinVersion}", packet.Version, packet.AccountId, Constants.MinSupportedVersion);
            return;
        }
        if (packet.IsRequest)
            HandleRequest(packet, data);
        else
            HandleResponse(packet, data);
    }

    private static void HandleRequest(DiscoveryPacket packet, ReceiveUserData data)
    {
        Log.Debug("HandleRequest: Packet: {packet}, rc: {data}", packet, data);

        // Not a broadcast we dont care about it.
        if (data.UnconnectedMessage is not LiteNetLib.UnconnectedMessageType.Broadcast)
            return;

        // Broadcast not running, we dont care about the discovery (MAIN are not the target audience.)
        if (!BroadcastManager.Instance.IsRunning)
            return;

        // Ignore if its our account.
        if (NetworkSettings.Instance.Account.AccountId == packet.AccountId)
            return;

        PeerAccount.TryAdd(packet.AccountId, data.EndPoint);

        DiscoveryPacket discoveryResponsePacket = new()
        { 
            AccountId = packet.AccountId,
            IsRequest = false
        };
        NetDataWriter writer = new();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref discoveryResponsePacket);
        BroadcastManager.Instance.SendUnconnectedMessage(writer, data.EndPoint);
    }

    private static void HandleResponse(DiscoveryPacket packet, ReceiveUserData data)
    {
        Log.Debug("HandleResponse: Packet: {packet}, rc: {data}", packet, data);

        // Not a basic message we dont care about it.
        if (data.UnconnectedMessage is not LiteNetLib.UnconnectedMessageType.BasicMessage)
            return;

        // BroadcastManager not running, we dont care about the response (Main are the target audience.)
        if (!BroadcastManager.Instance.IsRunning)
            return;

        BroadcastManager.Instance.Connect(data.EndPoint, Constants.BroadcastKey);
    }
}

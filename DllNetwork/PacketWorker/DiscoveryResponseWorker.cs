using DllNetwork.Main;
using Serilog;

namespace DllNetwork.PacketWorker;

public static partial class Workers
{
    public static void DiscoveryResponse(DiscoveryResponsePacket packet, ReceiveUserData data)
    {
        // Not a basic message we dont care about it.
        if (data.UnconnectedMessage is not LiteNetLib.UnconnectedMessageType.BasicMessage)
            return;

        // MainManager not running, we dont care about the response (Main are the target audience.)
        if (!MainManager.Instance.IsRunning)
            return;

        Log.Debug("Packet: {packet}, rc: {data}", packet, data);
        MainManager.Instance.Connect(data.EndPoint, Constants.BroadcastKey);
    }
}

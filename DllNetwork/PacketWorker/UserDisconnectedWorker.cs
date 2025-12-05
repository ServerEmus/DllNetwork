using Serilog;

namespace DllNetwork.PacketWorker;

public static partial class Workers
{
    public static void UserDisconnected(UserDisconnectedPacket packet, ReceiveUserData data)
    {
        Log.Debug("Packet: {packet}, rc: {data}", packet, data);

        PeerAccount.Remove(packet.AccountId);
    }
}

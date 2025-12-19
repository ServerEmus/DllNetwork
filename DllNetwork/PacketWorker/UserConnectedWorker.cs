using DllNetwork.Main;
using Serilog;
using System.Net;

namespace DllNetwork.PacketWorker;

public static partial class Workers
{
    public static void UserConnected(UserConnectedPacket packet, ReceiveUserData data)
    {
        Log.Debug("UserConnected: Packet: {packet}, rc: {data}", packet, data);

        PeerAccount.TryAdd(packet.AccountId, data.Peer);
    }
}

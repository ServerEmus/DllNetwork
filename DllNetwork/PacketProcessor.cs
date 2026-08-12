using DllNetwork.Broadcast;
using DllNetwork.Settings;
using LiteNetLib.Utils;
using Serilog;
using System.Net;

namespace DllNetwork;

/// <summary>
/// Provides a LiteNetLib packet processor with the default processors initialzed with.
/// </summary>
public static class PacketProcessor
{
    /// <summary>
    /// The networking packet processor.
    /// </summary>
    public static readonly NetPacketProcessor Processor = new();

    static PacketProcessor()
    {
        Processor.SubscribeNetSerializable<EmptyPacket, ReceiveData>(ReceiveEmpty);
        Processor.SubscribeNetSerializable<BroadcastPacket, IPEndPoint>(ReceiveBroadcastPacket);
    }

    private static void ReceiveBroadcastPacket(BroadcastPacket packet, IPEndPoint point)
    {
        // We skip our current.
        if (packet.Id == NetworkSettings.Instance.Account.AccountId)
        {
            return;
        }

        // If we have it cached just ignore.
        if (AccountStorage.AccountIdList.Contains(packet.Id))
        {
            return;
        }

        NetworkLog.Logger.Debug("BroadcastPacket received! {data} {point}", packet, point);

        foreach (var ip in packet.Addresses.Select(IPAddress.Parse))
        {
            PingHelper.PingAddress(packet.Id, ip, (id, ip, rtt) =>
            {
                AccountStorage.SetAddress(id, ip, rtt);
                BroadcastUdp.AddBroadcast(new()
                {
                    AccountId = id,
                    Addresses = [ip.ToString()],
                    Port = packet.ConnectPort,
                });
            });
        }

        // We sending our packet again.
        BroadcastUdp.Start();
    }

    private static void ReceiveEmpty(EmptyPacket packet, ReceiveData data)
    {
        NetworkLog.Logger.Debug("Empty Packet receveied! {data}", data);
    }
}

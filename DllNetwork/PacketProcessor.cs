using LiteNetLib;
using LiteNetLib.Utils;
using DllNetwork.Broadcast;
using Serilog;
using System.Net;

namespace DllNetwork;

public readonly struct ReceiveData
{
    public readonly NetPeer Peer;
    public readonly string AccountId;
    public readonly byte Channel;
    public readonly DeliveryMethod Delivery;

    public ReceiveData(NetPeer peer, byte channel, DeliveryMethod delivery)
    {
        Peer = peer;
        Channel = channel;
        Delivery = delivery;

        NetPeerStore.TryGetFromPeerId(peer.Id, out AccountId);
    }

    public override string ToString()
    {
        return $"{Peer} {AccountId} {Channel} {Delivery}";
    }
}

public readonly struct EmptyPacket() : INetSerializable
{
    public static readonly EmptyPacket Empty = new();

    public readonly void Deserialize(NetDataReader reader) { }

    public readonly void Serialize(NetDataWriter writer) { }
}

public struct BroadcastPacket : INetSerializable
{
    public string Id;
    public string[] Addresses;
    public int ConnectPort;

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetString();
        Addresses = reader.GetStringArray();
        ConnectPort = reader.GetInt();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.PutArray(Addresses);
        writer.Put(ConnectPort);
    }

    public readonly override string ToString()
    {
        return $"{Id} {ConnectPort} {string.Join(", ", Addresses)}";
    }
}


public static class PacketProcessor
{
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
            return;

        // If we have it cached just ignore.
        if (NetPeerStore.AccountIdList.Contains(packet.Id))
            return;

        Log.Information("BroadcastPacket receveied! {data} {point}", packet, point);

        foreach (var address in packet.Addresses)
        {
            var ip = IPAddress.Parse(address);
            PingHelper.PingAddress(packet.Id, ip, (id, ip, rtt) =>
            {
                NetPeerStore.SetAddress(id, ip, rtt);
                BroadcastUdp.AcceptedBroadcasts.Add(new()
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
        Log.Information("Empty Packet receveied! {data}", data);
    }
}

using DllNetwork.Packets;
using LiteNetLib.Utils;

namespace DllNetwork;

public static class PacketProcessor
{
    public static NetPacketProcessor Processor { get; }

    static PacketProcessor()
    {
        Processor = new();
        Processor.RegisterNestedType<DiscoveryPacket>();;
        Processor.RegisterNestedType<UserConnectedPacket>();
        Processor.RegisterNestedType<UserDisconnectedPacket>();

        Processor.SubscribeNetSerializable<DiscoveryPacket, ReceiveUserData>(PacketWorker.Workers.Discovery);
        Processor.SubscribeNetSerializable<UserConnectedPacket, ReceiveUserData>(PacketWorker.Workers.UserConnected);
        Processor.SubscribeNetSerializable<UserDisconnectedPacket, ReceiveUserData>(PacketWorker.Workers.UserDisconnected);
    }
}

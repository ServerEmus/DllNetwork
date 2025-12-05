using DllNetwork.Packets;
using LiteNetLib.Utils;

namespace DllNetwork;

public static class PacketProcessor
{
    public static NetPacketProcessor Processor { get; }

    static PacketProcessor()
    {
        Processor = new();
        Processor.RegisterNestedType<DiscoveryRequestPacket>();
        Processor.RegisterNestedType<DiscoveryResponsePacket>();
        Processor.RegisterNestedType<UserConnectedPacket>();
        Processor.RegisterNestedType<UserDisconnectedPacket>();

        Processor.SubscribeNetSerializable<DiscoveryRequestPacket, ReceiveUserData>(PacketWorker.Workers.DiscoveryRequest);
        Processor.SubscribeNetSerializable<DiscoveryResponsePacket, ReceiveUserData>(PacketWorker.Workers.DiscoveryResponse);
        Processor.SubscribeNetSerializable<UserConnectedPacket, ReceiveUserData>(PacketWorker.Workers.UserConnected);
        Processor.SubscribeNetSerializable<UserDisconnectedPacket, ReceiveUserData>(PacketWorker.Workers.UserDisconnected);
    }
}

using LiteNetLib.Utils;

namespace DllNetwork.Packets;

public struct DiscoveryResponsePacket : INetSerializable
{
    public readonly void Deserialize(NetDataReader reader) { }

    public readonly void Serialize(NetDataWriter writer) { }

    public readonly override string ToString()
    {
        return "DiscoveryResponse";
    }
}
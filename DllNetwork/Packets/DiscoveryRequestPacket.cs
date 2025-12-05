using LiteNetLib.Utils;

namespace DllNetwork.Packets;

public struct DiscoveryRequestPacket : INetSerializable
{
    public string AccountId;

    public void Deserialize(NetDataReader reader)
    {
        AccountId = reader.GetString();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(AccountId);
    }

    public readonly override string ToString()
    {
        return $"AccountId: {AccountId}";
    }
}

using LiteNetLib.Utils;

namespace DllNetwork.Packets;

public struct DiscoveryPacket : INetSerializable
{
    public int Version;
    public bool IsRequest;
    public string AccountId;

    public void Deserialize(NetDataReader reader)
    {
        Version = reader.GetInt();
        IsRequest = reader.GetBool();
        AccountId = reader.GetString();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Constants.Version);
        writer.Put(IsRequest);
        writer.Put(AccountId);
    }

    public readonly override string ToString()
    {
        return $"AccountId: {AccountId} IsRequest : {IsRequest}";
    }
}

using LiteNetLib.Utils;

namespace DllNetwork.Packets;

public struct DiscoveryPacket : INetSerializable
{
    public int Version;
    public bool IsRequest;
    public string AccountId;
    public List<string> Addresses;

    public void Deserialize(NetDataReader reader)
    {
        Version = reader.GetInt();
        IsRequest = reader.GetBool();
        AccountId = reader.GetString();
        int AddressesCount = reader.GetInt();
        Addresses = new(AddressesCount);
        for (int i = 0; i < AddressesCount; i++)
        {
            Addresses.Add(reader.GetString());
        }
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Constants.Version);
        writer.Put(IsRequest);
        writer.Put(AccountId);
        writer.Put(Addresses.Count);
        foreach (string address in Addresses)
            writer.Put(address);
    }

    public readonly override string ToString()
    {
        return $"AccountId: {AccountId} IsRequest : {IsRequest} {(IsRequest ? string.Join(", ",Addresses) : string.Empty)}";
    }
}

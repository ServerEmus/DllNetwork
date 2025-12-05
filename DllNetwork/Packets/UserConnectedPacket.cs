using LiteNetLib.Utils;

namespace DllNetwork.Packets;

public struct UserConnectedPacket : INetSerializable
{
    public string AccountId;
    public string IP;
    public void Deserialize(NetDataReader reader)
    {
        AccountId = reader.GetString();
        IP = reader.GetString();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(AccountId);
        writer.Put(IP);
    }

    public readonly override string ToString()
    {
        return $"[C] AccountId: {AccountId} IP: {IP}";
    }
}

using LiteNetLib.Utils;

namespace NetworkTest.CustomPacket;

public struct MessagePacket : INetSerializable
{
    public string Message;
    public void Deserialize(NetDataReader reader)
    {
        Message = reader.GetString();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Message);
    }
}

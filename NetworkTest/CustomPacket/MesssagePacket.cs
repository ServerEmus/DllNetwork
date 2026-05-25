using DllNetwork;
using LiteNetLib.Utils;
using Serilog;

namespace NetworkTest.CustomPacket;

public struct MessagePacket : INetSerializable
{
    public string Message;

    internal static void OnReceived(MessagePacket packet, ReceiveData data)
    {
        Log.Information("Message Packet receveied! {data} {packet}", data, packet);
    }

    public void Deserialize(NetDataReader reader)
    {
        Message = reader.GetString();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Message);
    }

    public readonly override string ToString()
    {
        return $"{Message}";
    }
}
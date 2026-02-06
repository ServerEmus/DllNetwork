using DllNetwork.Formatters;
using EIV_Pack;

namespace DllNetwork.Packets;

[EIV_Packable]
public partial class HeartBeatPacket : INetworkPacket
{
    public byte PacketId => (byte)PacketIdType.Heartbeat;

    public DateTime SentTime { get; set; } = DateTime.UtcNow;

    static HeartBeatPacket()
    {
        FormatterProvider.Register<HeartBeatPacket>();
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Heartbeat)
            return;

        HeartBeatPacket? packet = null;
        DeserializePackable(ref reader, ref packet);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Heartbeat)
            return;

        HeartBeatPacket? packet = (HeartBeatPacket?)value;
        SerializePackable(ref writer, in packet);
    }
}

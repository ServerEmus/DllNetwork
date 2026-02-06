using DllNetwork.Formatters;
using EIV_Pack;

namespace DllNetwork.Packets;

[EIV_Packable]
public partial class HeartBeatPacket : INetworkPacket
{
    public byte PacketId => (byte)PacketIdType.Heartbeat;

    public DateTimeOffset SentTime { get; set; } = DateTimeOffset.UtcNow;

    static HeartBeatPacket()
    {
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
        FormatterProvider.Register<HeartBeatPacket>();
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Heartbeat)
            return;

        HeartBeatPacket packet = new();
        DeserializePackable(ref reader, ref packet!);
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

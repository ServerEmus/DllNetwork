using DllNetwork.Formatters;
using EIV_Pack;
using System.Net;

namespace DllNetwork.Packets;

[EIV_Packable]
public partial class AnnouncePacket : INetworkPacket
{
    public byte PacketId => (byte)PacketIdType.Announce;
    public bool IsPing { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public PortStruct Port { get; set; } = new();
    public List<IPAddress> Addresses { get; set; } = [];
    public string AnnounceInformation { get; set; } = string.Empty;

    static AnnouncePacket()
    {
        FormatterProvider.Register<AnnouncePacket>();
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Announce)
            return;

        AnnouncePacket? packet = null;
        DeserializePackable(ref reader, ref packet);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Announce)
            return;

        AnnouncePacket? packet = (AnnouncePacket?)value;
        SerializePackable(ref writer, in packet);
    }

    public override string ToString()
    {
        return $"Ping? {IsPing} AID: {AccountId}, Port: {Port}, IPs: [{string.Join(", ", Addresses)} {AnnounceInformation}";
    }
}

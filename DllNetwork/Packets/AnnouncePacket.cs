using DllNetwork.Formatters;
using EIVPack;
using EIVPack.Formatters;
using System.Net;

namespace DllNetwork.Packets;

public partial class AnnouncePacket : INetworkPacket, IPackable<AnnouncePacket>, IFormatter<AnnouncePacket>
{
    public byte PacketId => (byte)PacketIdType.Announce;
    public bool IsPing { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public PortStruct Port { get; set; } = new();
    public List<IPAddress> Addresses { get; set; } = [];
    public string AnnounceInformation { get; set; } = string.Empty;

    static AnnouncePacket()
    {
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
        FormatterProvider.Register<AnnouncePacket>();
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Announce)
            return;

        AnnouncePacket packet = new();
        DeserializePackable(ref reader, ref packet!);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Announce)
            return;

        AnnouncePacket? packet = (AnnouncePacket?)value;
        SerializePackable(ref writer, in packet);
    }

    public static void RegisterFormatter()
    {
        if (!FormatterProvider.IsRegistered<AnnouncePacket>())
        {
            FormatterProvider.Register(new AnnouncePacket());
        }

        if (!FormatterProvider.IsRegistered<AnnouncePacket[]>())
        {
            FormatterProvider.Register(new ArrayFormatter<AnnouncePacket>());
        }
    }

    public static void DeserializePackable(ref PackReader reader, scoped ref AnnouncePacket? value)
    {
        if (!reader.TryReadSmallHeader(out byte header) || header != 5)
        {
            value = null;
            return;
        }

        value ??= new();

        value.IsPing = reader.ReadUnmanaged<bool>();
        value.AccountId = reader.ReadString()!;
        value.Port = reader.ReadUnmanaged<PortStruct>();
        value.Addresses = reader.ReadValue<List<IPAddress>>() ?? [];
        value.AnnounceInformation = reader.ReadString()!;
    }

    public static void SerializePackable(ref PackWriter writer, scoped ref readonly AnnouncePacket? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(5);
        writer.WriteUnmanaged(value.IsPing);
        writer.WriteString(value.AccountId);
        writer.WriteUnmanaged(value.Port);
        writer.WriteValue(value.Addresses);
        writer.WriteString(value.AnnounceInformation);
    }

    public void Deserialize(ref PackReader reader, scoped ref AnnouncePacket? value)
    {
        DeserializePackable(ref reader, ref value);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly AnnouncePacket? value)
    {
        SerializePackable(ref writer, in value);
    }

    public override string ToString()
    {
        return $"Ping? {IsPing} AID: {AccountId}, Port: {Port}, IPs: [{string.Join(", ", Addresses)} {AnnounceInformation}";
    }
}

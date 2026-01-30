using DllNetwork.Formatters;
using EIVPack;
using EIVPack.Formatters;

namespace DllNetwork.Packets;

public class HandshakePacket : INetworkPacket, IPackable<HandshakePacket>, IFormatter<HandshakePacket>
{
    public byte PacketId => (byte)PacketIdType.Handshake;
    /// <summary>
    /// Current version of this library.
    /// </summary>
    public uint Version { get; set; } = Constants.DLL_VERSION;

    /// <summary>
    /// The AccountId of the sender.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The handshake key.
    /// </summary>
    public string HandshakeKey { get; set; } = string.Empty;


    static HandshakePacket()
    {
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
        FormatterProvider.Register<HandshakePacket>();
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Handshake)
            return;

        HandshakePacket packet = new();
        DeserializePackable(ref reader, ref packet!);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Handshake)
            return;

        HandshakePacket? packet = (HandshakePacket?)value;
        SerializePackable(ref writer, in packet);
    }
    public static void RegisterFormatter()
    {
        if (!FormatterProvider.IsRegistered<HandshakePacket>())
        {
            FormatterProvider.Register(new HandshakePacket());
        }

        if (!FormatterProvider.IsRegistered<HandshakePacket[]>())
        {
            FormatterProvider.Register(new ArrayFormatter<HandshakePacket>());
        }
    }

    public static void DeserializePackable(ref PackReader reader, scoped ref HandshakePacket? value)
    {
        if (!reader.TryReadSmallHeader(out byte header) || header != 3)
        {
            value = null;
            return;
        }

        value ??= new();

        value.Version = reader.ReadUnmanaged<uint>();
        value.AccountId = reader.ReadString()!;
        value.HandshakeKey = reader.ReadString()!;
    }

    public static void SerializePackable(ref PackWriter writer, scoped ref readonly HandshakePacket? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(3);
        writer.WriteUnmanaged(value.Version);
        writer.WriteString(value.AccountId);
        writer.WriteString(value.HandshakeKey);
    }

    public void Deserialize(ref PackReader reader, scoped ref HandshakePacket? value)
    {
        DeserializePackable(ref reader, ref value);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly HandshakePacket? value)
    {
        SerializePackable(ref writer, in value);
    }
}

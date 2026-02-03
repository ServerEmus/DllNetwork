using DllNetwork.Formatters;
using EIV_Pack;

namespace DllNetwork.Packets;

[EIV_Packable]
public partial class HandshakePacket : INetworkPacket
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
}

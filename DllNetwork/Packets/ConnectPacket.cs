using DllNetwork.Formatters;
using EIV_Pack;

namespace DllNetwork.Packets;

[EIV_Packable]
public partial class ConnectPacket : INetworkPacket
{
    /// <inheritdoc/>
    public byte PacketId => (byte)PacketIdType.Connect;

    /// <summary>
    /// Current version of this library.
    /// </summary>
    public uint Version { get; set; } = Constants.DLL_VERSION;

    /// <summary>
    /// The handshake key.
    /// </summary>
    public string HandshakeKey { get; set; } = string.Empty;

    static ConnectPacket()
    {
        FormatterProvider.Register<ConnectPacket>();
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Connect)
            return;

        ConnectPacket? packet = null;
        DeserializePackable(ref reader, ref packet);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.Connect)
            return;

        ConnectPacket? packet = (ConnectPacket?)value;
        SerializePackable(ref writer, in packet);
    }
}

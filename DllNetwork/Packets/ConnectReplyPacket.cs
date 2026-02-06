using DllNetwork.Formatters;
using EIV_Pack;

namespace DllNetwork.Packets;

public enum DenyReason : byte
{
    None,
    VersionMissmatch,
    HandshakeKeyMissmatch,
}


[EIV_Packable]
public partial class ConnectReplyPacket : INetworkPacket
{
    public byte PacketId => (byte)PacketIdType.ConnectReply;

    /// <summary>
    /// Reason the handshake got denied.
    /// </summary>
    public DenyReason DenyReason { get; set; } = DenyReason.None;


    static ConnectReplyPacket()
    {
        INetworkPacketFormatter.Instance.WritePacket += Instance_WritePacket;
        INetworkPacketFormatter.Instance.GetPacket += Instance_GetPacket;
        FormatterProvider.Register<ConnectReplyPacket>();
    }

    private static void Instance_GetPacket(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.ConnectReply)
            return;

        ConnectReplyPacket packet = new();
        DeserializePackable(ref reader, ref packet!);
        value = packet;
    }

    private static void Instance_WritePacket(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (PacketId != (byte)PacketIdType.ConnectReply)
            return;

        ConnectReplyPacket? packet = (ConnectReplyPacket?)value;
        SerializePackable(ref writer, in packet);
    }
}

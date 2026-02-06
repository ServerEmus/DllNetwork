namespace DllNetwork.Packets;

public interface INetworkPacket
{
    /// <summary>
    /// Identifier for this Network packet.
    /// </summary>
    public byte PacketId { get; }
}


public enum PacketIdType : byte
{
    None = 0,
    Announce = 1,
    Connect = 2,
    ConnectReply = 3,
    Heartbeat = 4,
}
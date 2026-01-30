namespace DllNetwork.Packets;

public interface INetworkPacket
{
    public byte PacketId { get; }
}


public enum PacketIdType : byte
{
    None = 0,
    Announce = 1,
    Handshake = 2,
}
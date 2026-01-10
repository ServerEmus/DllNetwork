using MemoryPack;

namespace DllNetwork.Packets;

[MemoryPackable]
public partial class HandshakePacket : INetworkPacket
{
    public int Version { get; set; } = Constants.Version;
    public string AccountId { get; set; } = string.Empty;
    public string HandshakeKey { get; set; } = string.Empty;
}

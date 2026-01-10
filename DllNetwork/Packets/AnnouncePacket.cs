using MemoryPack;
using System.Net;

namespace DllNetwork.Packets;

[MemoryPackable]
public partial class AnnouncePacket : INetworkPacket
{
    public bool IsPing { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public PortStruct Port { get; set; } = new();
    public List<IPAddress> Addresses { get; set; } = [];
    public string AnnounceInformation { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Ping? {IsPing} AID: {AccountId}, Port: {Port}, IPs: [{string.Join(", ", Addresses)} {AnnounceInformation}";
    }
}

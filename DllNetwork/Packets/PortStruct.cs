using MemoryPack;

namespace DllNetwork.Packets;

[MemoryPackable]
public partial struct PortStruct
{
    public bool HasIpv6;
    public int TcpPort;
    public int UdpPort;
    public int BroadcastPort;

    public readonly override string ToString()
    {
        return $"{HasIpv6} {TcpPort} {UdpPort} {BroadcastPort}";
    }
}
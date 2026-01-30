namespace DllNetwork.Packets;

public struct PortStruct
{
    public bool HasIpv6;
    public int TcpPort;
    public int UdpPort;
    public int BroadcastPort;

    public readonly int GetPort(PortType portType)
    {
        return portType switch
        {
            PortType.None => 0,
            PortType.Udp => UdpPort,
            PortType.Tcp => TcpPort,
            PortType.Broadcast => BroadcastPort,
            _ => 0,
        };
    }

    public readonly override string ToString()
    {
        return $"{HasIpv6} {TcpPort} {UdpPort} {BroadcastPort}";
    }
}
using System.Net;

namespace DllSocket.Test;

internal class TestConst
{
    // Any Bind
    public static readonly IPEndPoint AnyClient = new(IPAddress.Any, 0);
    public static readonly IPEndPoint AnyV6Client = new(IPAddress.IPv6Any, 0);

    public const int UDP_Port = 7776;
    public const int Broadcast_Port = 7777;
    public const int TCP_Port = 7778;

    // UDP related test
    public static readonly IPEndPoint AnyHost_UPD = new(IPAddress.Any, UDP_Port);
    public static readonly IPEndPoint AnyV6Host_UPD = new(IPAddress.IPv6Any, UDP_Port);
    public static readonly IPEndPoint ConnectV4_UPD = new(IPAddress.Loopback, UDP_Port);
    public static readonly IPEndPoint ConnectV6_UPD = new(IPAddress.IPv6Loopback, UDP_Port);

    // Broadcast related test
    public static readonly IPEndPoint Broadcast = new(IPAddress.Broadcast, Broadcast_Port);
    public static readonly IPEndPoint AnyHost = new(IPAddress.Any, Broadcast_Port);
    public static readonly IPEndPoint AnyV6Host = new(IPAddress.IPv6Any, Broadcast_Port);
    public static readonly IPEndPoint ConnectV4 = new(IPAddress.Loopback, Broadcast_Port);
    public static readonly IPEndPoint ConnectV6 = new(IPAddress.IPv6Loopback, Broadcast_Port);

    // TCP related test
    public static readonly IPEndPoint AnyHost_TCP = new(IPAddress.Any, TCP_Port);
    public static readonly IPEndPoint AnyV6Host_TCP = new(IPAddress.IPv6Any, TCP_Port);
    public static readonly IPEndPoint ConnectV4_TCP = new(IPAddress.Loopback, TCP_Port);
    public static readonly IPEndPoint ConnectV6_TCP = new(IPAddress.IPv6Loopback, TCP_Port);
    public static readonly byte[] DataToSend = [0xFF, 0xAA, 0xFF, 0xBB];
}

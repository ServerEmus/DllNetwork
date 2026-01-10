using System.Net;

namespace DllNetwork;

public static class Constants
{
    public const int Version = 1;
    public const int MinSupportedVersion = 1;
    public static readonly EndPoint ReceiveEndpointV4 = new IPEndPoint(IPAddress.Any, 0);
    public static readonly EndPoint ReceiveEndpointV6 = new IPEndPoint(IPAddress.IPv6Any, 0);

}

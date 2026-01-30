using System.Net;

namespace DllNetwork;

public static class Constants
{
    public const uint DLL_VERSION = 1;
    public const uint DLL_MIN_SUPPORTED_VERSION = 1;
    public static readonly IPEndPoint ReceiveEndpointV4 = new(IPAddress.Any, 0);
    public static readonly IPEndPoint ReceiveEndpointV6 = new(IPAddress.IPv6Any, 0);

}

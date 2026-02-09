using System.Net;
using System.Net.Sockets;

namespace DllSocket;

public static class SocketExtension
{
    public static readonly IPEndPoint ReceiveEndpointV4 = new(IPAddress.Any, 0);
    public static readonly IPEndPoint ReceiveEndpointV6 = new(IPAddress.IPv6Any, 0);

    extension(Socket socket)
    {
        public string ToFancyString()
        {
            return $"{socket.AddressFamily} {socket.Available} {socket.Connected} {socket.ProtocolType} {socket.LocalEndPoint} {socket.RemoteEndPoint}";
        }
    }
}

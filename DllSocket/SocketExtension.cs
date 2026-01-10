using System.Net.Sockets;

namespace DllSocket;

public static class SocketExtension
{
    extension(Socket socket)
    {
        public string ToFancyString()
        {
            return $"{socket.AddressFamily} {socket.Available} {socket.Connected} {socket.ProtocolType} {socket.LocalEndPoint} {socket.RemoteEndPoint}";
        }
    }
}

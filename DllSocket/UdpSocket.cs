using System.Net.Sockets;

namespace DllSocket;

public class UdpSocket(bool enableIpv6 = true) : CoreSocket(SocketType.Dgram, ProtocolType.Udp, enableIpv6);

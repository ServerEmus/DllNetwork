using DllNetwork.SocketWorkers;
using DllSocket;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork;

public class MainNetwork
{
    public static MainNetwork Instance { get; } = new();
    public PortStruct NetworkPorts { get; private set; }

    public readonly NetworkSettings settings;
    public readonly bool Ipv6Enabled;
    public readonly BroadcastSocket broadcast;
    public readonly TcpSocket tcpServer;
    public readonly TcpSocket tcpClient;
    public readonly UdpSocket udp;

    internal readonly List<SocketAddress> SocketAddresses = [];
    internal readonly List<IPAddress> MyIpAddresses = [];

    public BroadcastWork BroadcastWork { get; private set; } = default!;
    public UdpWork UdpWork { get; private set; } = default!;

    internal MainNetwork()
    {
        settings = NetworkSettings.Instance;
        Ipv6Enabled = settings.Manager.EnableIpv6;
        broadcast = new(Ipv6Enabled);
        broadcast.OnException += Broadcast_OnException;
        tcpServer = new(Ipv6Enabled);
        tcpServer.OnException += Tcp_OnException;
        tcpClient = new(Ipv6Enabled);
        tcpClient.OnException += Tcp_OnException;
        udp = new(Ipv6Enabled);
        udp.OnException += Udp_OnException;
        for (int i = settings.Broadcast.BroadcastPort; i < settings.Broadcast.EndRangeBroadcastPort; i++)
        {
            SocketAddresses.Add(new IPEndPoint(IPAddress.Broadcast, i).Serialize());
        }
        MyIpAddresses = AddressHelper.GetInterfaceAddresses();

        if (!settings.Manager.EnableIpv6)
        {
            MyIpAddresses.RemoveAll(x => x.AddressFamily == AddressFamily.InterNetworkV6);
        }
    }

    private void Udp_OnException(Exception obj)
    {
        Log.Error("[MainNetwork.UDP] Excpetion: {ex}", obj);
    }

    private void Tcp_OnException(Exception obj)
    {
        Log.Error("[MainNetwork.TCP] Excpetion: {ex}", obj);
    }

    private void Broadcast_OnException(Exception obj)
    {
        Log.Error("[MainNetwork.Broadcast] Excpetion: {ex}", obj);
    }

    public void Start()
    {
        BroadcastWork = new(broadcast);
        UdpWork = new(udp, settings.Manager.MaxQueueSize, settings.Manager.HearthbeatInterval);

        broadcast.Start();
        int broadcastPort = AddressHelper.GetPort(settings.Broadcast.BroadcastPort, settings.Broadcast.EndRangeBroadcastPort, false);
        IPEndPoint broadCastEndPoint = new(IPAddress.Parse(settings.Binding.BindIpv4), broadcastPort);

        IPEndPoint? broadCastEndPointV6 = null;
        if (Ipv6Enabled)
            broadCastEndPointV6 = new(IPAddress.Parse(settings.Binding.BindIpv6), broadcastPort);
        broadcast.Bind(broadCastEndPoint, broadCastEndPointV6);

        tcpServer.Start();
        int tcpPort = AddressHelper.GetPort();
        IPEndPoint tcpEndPoint = new(IPAddress.Any, tcpPort);

        IPEndPoint? tcpEndPointV6 = null;
        if (Ipv6Enabled)
            tcpEndPointV6 = new(IPAddress.IPv6Any, tcpPort);
        tcpServer.Bind(tcpEndPoint, tcpEndPointV6);

        tcpClient.Start();

        udp.Start();
        int udpPort = AddressHelper.GetPort(isTcp: false);
        IPEndPoint udpEndPoint = new(IPAddress.Any, udpPort);

        IPEndPoint? udpEndPointV6 = null;
        if (Ipv6Enabled)
            udpEndPointV6 = new(IPAddress.IPv6Any, udpPort);
        udp.Bind(udpEndPoint, udpEndPointV6);

        NetworkPorts = new()
        { 
            HasIpv6 = Ipv6Enabled,
            TcpPort = tcpPort,
            UdpPort = udpPort,
            BroadcastPort = broadcastPort,
        };
    }

    public void Update()
    {
        broadcast.Update();
        BroadcastWork.Update();
        tcpServer.Update();
        tcpClient.Update();
        udp.Update();
        UdpWork.Update();
    }


    public void Stop()
    {
        broadcast.Stop();
        tcpServer.Stop();
        tcpClient.Stop();
        udp.Stop();
    }
}

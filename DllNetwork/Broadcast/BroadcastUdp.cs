using LiteNetLib.Utils;
using DllNetwork.Json;
using DllNetwork.Managers;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Broadcast;

public static class BroadcastUdp
{
    static readonly UdpClient udp;
    static readonly UdpClient? udpv6;
    static readonly NetDataWriter netDataWriter = new();
    public static readonly List<BroadcastJson> AcceptedBroadcasts = [];

    static BroadcastUdp()
    {
        // UDP v4
        udp = new()
        {
            EnableBroadcast = true,
        };


        int broadcastPort = AddressHelper.GetPort(NetworkSettings.Instance.Broadcast.BroadcastPort, NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort, false);
        IPEndPoint broadCastEndPoint = new(IPAddress.Any, broadcastPort);

        udp.Client.Bind(broadCastEndPoint);

        // UDP v6
        if (!NetworkSettings.Instance.Manager.EnableIpv6)
            return;

        udpv6 = new()
        {
            EnableBroadcast = true,
        };

        broadcastPort = AddressHelper.GetPort(NetworkSettings.Instance.Broadcast.BroadcastPort, NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort, false);
        broadCastEndPoint = new(IPAddress.IPv6Any, broadcastPort);

        udpv6.Client.Bind(broadCastEndPoint);
    }

    public static void Start()
    {
        BroadcastPacket packet = new()
        { 
            Id = NetworkSettings.Instance.Account.AccountId,
            Addresses = [.. AddressHelper.Addresses.Select(static x => x.ToString())],
            ConnectPort = ServerManager.Instance.Port,
        };

        PacketProcessor.Processor.WriteNetSerializable(netDataWriter, ref packet);

        var span = netDataWriter.AsReadOnlySpan();

        for (int port = NetworkSettings.Instance.Broadcast.BroadcastPort; port < NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort; port++)
        {
            IPEndPoint address = new(IPAddress.Broadcast, port);
            udp.Send(span, address);
            udpv6?.Send(span, address);
        }
    }

    public static void Stop()
    {

    }

    public static List<BroadcastJson> GetList()
    {
        foreach (var item in AcceptedBroadcasts)
        {
            PingHelper.ClearPingedAccount(item.AccountId);
        }

        List<BroadcastJson> normalized = [];

        foreach (var item in AcceptedBroadcasts)
        {
            if (!normalized.Exists(x => item.AccountId == x.AccountId))
            {
                normalized.Add(item);
                continue;
            }

            var found = normalized.FirstOrDefault(x => item.AccountId == x.AccountId);
            if (found == null)
            {
                continue;
            }

            found.Addresses.AddRange(item.Addresses);
        }

        AcceptedBroadcasts.Clear();
        AcceptedBroadcasts.AddRange(normalized);

        return AcceptedBroadcasts;
    }


    public static void UdpUpdate()
    {
        udp.ReceiveAsync().ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            NetDataReader reader = new(task.Result.Buffer);
            PacketProcessor.Processor.ReadPacket(reader, task.Result.RemoteEndPoint);
        });

        udpv6?.ReceiveAsync().ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            NetDataReader reader = new(task.Result.Buffer);
            PacketProcessor.Processor.ReadPacket(reader, task.Result.RemoteEndPoint);
        });
    }
}

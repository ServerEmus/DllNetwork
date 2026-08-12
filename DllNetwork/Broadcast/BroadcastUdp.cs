using DllNetwork.Json;
using DllNetwork.Managers;
using DllNetwork.Settings;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;

namespace DllNetwork.Broadcast;

/// <summary>
/// Provides broadcasting via udp.
/// </summary>
public static class BroadcastUdp
{
    private static readonly List<BroadcastAccount> AcceptedBroadcasts = [];
    private static readonly UdpClient Udp;
    private static readonly UdpClient Udpv6;
    private static readonly NetDataWriter NetDataWriter = new();
    private static bool isStarted = false;

    static BroadcastUdp()
    {
        Udp = new()
        {
            EnableBroadcast = true,
        };

        Udpv6 = new()
        {
            EnableBroadcast = true,
        };
    }

    /// <summary>
    /// Starts the udp clients and sends a <see cref="BroadcastPacket"/>.
    /// </summary>
    public static void Start()
    {
        if (!isStarted)
        {
            int broadcastPort = AddressHelper.GetPort(NetworkSettings.Instance.Broadcast.BroadcastPort, NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort, false);
            IPEndPoint v4Endpoint = new(IPAddress.Any, broadcastPort);

            Udp.Client.Bind(v4Endpoint);

            if (NetworkSettings.Instance.Manager.EnableIpv6)
            {
                broadcastPort = AddressHelper.GetPort(NetworkSettings.Instance.Broadcast.BroadcastPort, NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort, false);
                IPEndPoint v6Endpoint = new(IPAddress.IPv6Any, broadcastPort);

                Udpv6.Client.Bind(v6Endpoint);
            }

            isStarted = true;
        }

        BroadcastPacket packet = new()
        {
            Id = NetworkSettings.Instance.Account.AccountId,
            Addresses = [.. AddressHelper.Addresses.Select(static x => x.ToString())],
            ConnectPort = ServerManager.Instance.Port,
        };

        PacketProcessor.Processor.WriteNetSerializable(NetDataWriter, ref packet);

        var span = NetDataWriter.AsReadOnlySpan();

        for (int port = NetworkSettings.Instance.Broadcast.BroadcastPort; port < NetworkSettings.Instance.Broadcast.EndRangeBroadcastPort; port++)
        {
            IPEndPoint address = new(IPAddress.Broadcast, port);
            Udp.Send(span, address);
            Udpv6.Send(span, address);
        }
    }

    /// <summary>
    /// Stops the udp clients.
    /// </summary>
    public static void Stop()
    {
        Udp.Close();
        Udpv6.Close();
        isStarted = false;
    }

    /// <summary>
    /// Gets the broadcast accounts from the accepted list.
    /// </summary>
    /// <returns>The broadcast accounts.</returns>
    public static List<BroadcastAccount> GetList()
    {
        foreach (var item in AcceptedBroadcasts)
        {
            PingHelper.ClearPingedAccount(item.AccountId);
        }

        List<BroadcastAccount> normalized = [];

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

    /// <summary>
    /// Starts to receieve from the udp clients.
    /// </summary>
    public static void UdpReceive()
    {
        if (!isStarted)
        {
            return;
        }

        Udp.ReceiveAsync().ContinueWith(Receive);

        if (NetworkSettings.Instance.Manager.EnableIpv6)
        {
            Udpv6.ReceiveAsync().ContinueWith(Receive);
        }
    }

    /// <summary>
    /// Add <paramref name="account"/> into internal accepted accounts.
    /// </summary>
    /// <param name="account">The account to add.</param>
    internal static void AddBroadcast(BroadcastAccount account)
    {
        AcceptedBroadcasts.Add(account);
    }

    private static void Receive(Task<UdpReceiveResult> task)
    {
        if (!task.IsCompletedSuccessfully)
        {
            return;
        }

        NetDataReader reader = new(task.Result.Buffer);
        PacketProcessor.Processor.ReadPacket(reader, task.Result.RemoteEndPoint);
    }
}

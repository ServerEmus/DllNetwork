using DllNetwork.PacketProcessors;
using DllSocket;
using Serilog;
using System.Buffers;
using System.Net;

namespace DllNetwork;

public class BroadcastWork
{
    private readonly HandshakePacket Handshake;
    private readonly BroadcastSocket broadcastSocket;


    public BroadcastWork(BroadcastSocket socket)
    {
        broadcastSocket = socket;
        Handshake = new()
        {
            AccountId = MainNetwork.Instance.settings.Account.AccountId,
            HandshakeKey = MainNetwork.Instance.settings.Connection.HandshakeKey
        };
    }

    public AnnouncePacket? AnnouncePacket = new();


    public void BroadcastUpdate()
    {
        int av = broadcastSocket.Available;
        if (av == 0)
            return;

        foreach (var socketAddress in MainNetwork.Instance.SocketAddresses)
        {
            byte[] pool = ArrayPool<byte>.Shared.Rent(av);
            broadcastSocket.Receive(pool, socketAddress).AsTask().
                ContinueWith(async (completedTask) =>
                {
                    if (!completedTask.IsCompletedSuccessfully)
                    {
                        Log.Information("Task failed!");
                        ArrayPool<byte>.Shared.Return(pool, true);
                        return;
                    }
                    int actualReceived = completedTask.Result;
                    Log.Information("Bytes {len} received from {address} ", actualReceived, socketAddress);
                    Log.Debug("Received bytes in pool: {pool}", Convert.ToHexString(pool));
                    if (actualReceived != 0)
                    {
                        pool.Deserialize(ref AnnouncePacket);
                        await AnnounceWork(AnnouncePacket);
                    }
                    ArrayPool<byte>.Shared.Return(pool, true);
                });

        }
    }

    private async Task AnnounceWork(AnnouncePacket? announcePacket)
    {
        if (announcePacket == null)
            return;

        if (announcePacket.AccountId == NetworkSettings.Instance.Account.AccountId)
            return;

        Log.Information("Announce packet: {Ann}", announcePacket);

        NetworkAccount.AddPort(announcePacket.AccountId, announcePacket.Port);

        List<Task> PingTasks = [];
        if (announcePacket.IsPing)
        {
            foreach (var address in announcePacket.Addresses)
            {
                PingTasks.Add(PingHelper.PingAddressAsync(announcePacket.AccountId, address, NetworkAccount.AddAddress));
            }
        }

        ConnectAsyncWork(announcePacket, PingTasks).Start();

        announcePacket.AccountId = NetworkSettings.Instance.Account.AccountId;
        announcePacket.Addresses = MainNetwork.Instance.MyIpAddresses;
        announcePacket.IsPing = false;
        announcePacket.Port = MainNetwork.Instance.NetworkPorts;
        announcePacket.AnnounceInformation = "Reply!";
        byte[] data = announcePacket.Serialize();

        await broadcastSocket.Send(data, new IPEndPoint(IPAddress.Broadcast, MainNetwork.Instance.NetworkPorts.BroadcastPort));
    }

    private async Task ConnectAsyncWork(AnnouncePacket announcePacket, List<Task> tasks)
    {
        await Task.WhenAll(tasks);

        if (NetworkAccount.TryGetEndpoint(announcePacket.AccountId, PortType.Udp, out var endPoint))
        {
            MainProcessor.DenyProcessingEndpoints.Remove(endPoint);
        }

        MainNetwork.Instance.UdpWork.Send(Handshake, announcePacket.AccountId);
    }

    public void SendAnnounce()
    {
        AnnouncePacket = new()
        {
            AccountId = NetworkSettings.Instance.Account.AccountId,
            IsPing = true,
            Port = MainNetwork.Instance.NetworkPorts,
            AnnounceInformation = "Ping Hello!",
            Addresses = MainNetwork.Instance.MyIpAddresses
        };
        byte[] data = AnnouncePacket.Serialize();
        foreach (var socketAddress in MainNetwork.Instance.SocketAddresses)
        {
            broadcastSocket.Send(data, socketAddress).AsTask().
                ContinueWith((completedTask) =>
                {
                    Log.Information("Announce {packet} send to {address} {len}", AnnouncePacket, socketAddress, completedTask.Result);
                });
        }

    }
}

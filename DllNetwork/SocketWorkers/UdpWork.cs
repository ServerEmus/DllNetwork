using DllNetwork.PacketProcessors;
using DllNetwork.Packets;
using DllSocket;
using Serilog;
using System.Net;

namespace DllNetwork.SocketWorkers;

public class UdpWork(UdpSocket socket, int maxPacketQueue) : ISocketWorker
{
    public PortType PortType => PortType.Udp;
    public CoreSocket Socket => udp;

    public static readonly Queue<KeyValuePair<INetworkPacket, string>> PacketQueue = new();
    public static readonly Dictionary<string, DateTimeOffset> LastHeartBeatReceived = [];

    private static readonly List<(HeartBeatPacket, DateTimeOffset, string)> HeartBeatSendList = [];

    private readonly int MaxPacketQueue = maxPacketQueue;
    private readonly UdpSocket udp = socket;
    private readonly IPEndPoint SenderEndPoint = new(IPAddress.Any, 0);
    private readonly Memory<byte> ReceiveBuffer = new byte[CoreSocket.BufferSize];

    public void Update()
    {
        UdpReceive();
        UdpSend();
    }

    private void UdpReceive()
    {
        IPEndPoint receive = Constants.ReceiveEndpointV4;
        int available = 0;
        if (udp.EnableIpv6 && udp.socketv6 != null)
        {
            available = udp.socketv6.Available;
            receive = Constants.ReceiveEndpointV6;
        }
        else if (udp.socketv4 != null)
        {
            available = udp.socketv4.Available;
        }

        if (available == 0)
            return;

        udp.Receive(ReceiveBuffer, receive).AsTask().
            ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Information("Task failed!");
                    return;
                }
                var receiveFromResult = completedTask.Result;
                Log.Information("Bytes {len} received from {address} (or {address2})", receiveFromResult.ReceivedBytes, receive, receiveFromResult.RemoteEndPoint);

                if (!MainProcessor.CanProcess(receive, out string accountId))
                    return;

                MainProcessor.ReceiveProcess(this, ReceiveBuffer[..receiveFromResult.ReceivedBytes], receive, accountId);
            });
    }

    private void UdpSend()
    {
        int packetQueued = 0;

        while (PacketQueue.TryDequeue(out var packet) && packetQueued < MaxPacketQueue)
        {
            packetQueued++;

            // TODO: We can optimize this by batching packets to the same account together, but for now we just send them one by one

            if (packet.Key is HeartBeatPacket heartBeatPacket)
            {
                var sentTime = heartBeatPacket.SentTime;
                var now = DateTimeOffset.UtcNow;
                var timeDiff = (now - sentTime).TotalSeconds;
                Log.Debug("HB debug2 {timediff} {time1} {time2}", timeDiff, now, sentTime);
                if ((now - sentTime).Seconds < 5)
                {
                    HeartBeatSendList.Add((heartBeatPacket, DateTimeOffset.UtcNow.AddSeconds(5), packet.Value));
                    continue;
                }
            }

            Send(packet.Key, packet.Value);
        }

        for (int i = 0; i < HeartBeatSendList.Count; i++)
        {
            var hb = HeartBeatSendList[i];
            var now = DateTimeOffset.UtcNow;
            var timeDiff = (now - hb.Item2).TotalSeconds;
            Log.Debug("HB debug3 {timediff} {time1} {time2}", timeDiff, now, hb.Item2);
            if ((now - hb.Item2).Seconds < 5)
            {
                Send(hb.Item1, hb.Item3);
                HeartBeatSendList.RemoveAt(i);
            }
        }
    }


    public void AddPacketQueue(INetworkPacket networkPacket, string userId)
    {
        PacketQueue.Enqueue(new KeyValuePair<INetworkPacket, string>(networkPacket, userId));
    }

    public void Send<T>(T packet, string accountId) where T : INetworkPacket
    {
        var bytes = packet.Serialize();
        if (!NetworkAccount.TryGetFirstAddress(accountId, out var address) || address == null)
        {
            Log.Warning("No address found for account ID {accountId}", accountId);
            return;
        }

        if (!NetworkAccount.TryGetPort(accountId, out var port) || port.UdpPort == 0)
        {
            Log.Warning("No ports found for account ID {accountId}", accountId);
            return;
        }

        SenderEndPoint.Address = address;
        SenderEndPoint.Port = port.UdpPort;
        udp.Send(bytes, SenderEndPoint).AsTask().
            ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Error("Packet could not be sent! {Ex}", completedTask.Exception);
                    return;
                }
            });
    }

}

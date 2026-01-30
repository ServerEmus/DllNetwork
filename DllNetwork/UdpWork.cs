using DllNetwork.PacketProcessors;
using DllSocket;
using Serilog;
using System.Net;

namespace DllNetwork;

public class UdpWork(UdpSocket socket)
{
    private readonly UdpSocket udp = socket;
    public Memory<byte> ReceiveBuffer = new byte[CoreSocket.BufferSize];
    private readonly IPEndPoint SenderEndPoint = new(IPAddress.Any, 0);

    public void UdpReceive()
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

                MainProcessor.ReceiveProcess(ReceiveBuffer[..receiveFromResult.ReceivedBytes], receive, accountId);
            });
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

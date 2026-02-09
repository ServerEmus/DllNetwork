using DllNetwork.PacketProcessors;
using DllSocket;
using Serilog;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;

namespace DllNetwork.SocketWorkers;

public struct UserIdSocket
{
    public string UserId { get; set; }
    public Socket Socket { get; set; }
}

public class TcpWork(TcpSocket tcpServer, TcpSocket tcpClient) : ISocketWorker
{
    private readonly TcpSocket server = tcpServer;
    private readonly TcpSocket client = tcpClient;
    public PortType PortType => PortType.Tcp;
    public CoreSocket Socket => server;
    public static readonly Queue<KeyValuePair<INetworkPacket, string>> PacketQueue = new();

    public static readonly List<UserIdSocket> UserIdToSocket = [];

    public void Update()
    {
        ReceiveUpdate();
        SendUpdate();
    }

    public void AddPacketQueue(INetworkPacket networkPacket, string userId)
    {
        PacketQueue.Enqueue(new(networkPacket, userId));
    }

    public void Connect(string accountId)
    {
        if (!NetworkAccount.TryGetEndpoint(accountId, PortType.Tcp, out var address) || address == null)
        {
            Log.Warning("No address found for account ID {accountId}", accountId);
            return;
        }

        client.Connect(address.Address, address.Port);
    }

    public void Connected(Socket connectedSocket)
    {
        Log.Information("New TCP connection from {address} ({fancy})", connectedSocket.RemoteEndPoint, connectedSocket.ToFancyString());

        server.Send(connectedSocket, ConnectPacket.MyPacket.Serialize()).AsTask()
            .ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Error("ConnectPacket could not be sent! {Ex}", completedTask.Exception);
                    return;
                }

                Log.Debug("ConnectPacket sent!");
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
        /*
        SenderEndPoint.Address = address;
        SenderEndPoint.Port = port.UdpPort;
        Log.Debug("Sending packet {packet} to address: {address} ({acc})", bytes, SenderEndPoint, accountId);
        socket.Send(bytes).AsTask().
            ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Error("Packet could not be sent! {Ex}", completedTask.Exception);
                    return;
                }
            });
        */
    }


    private void ReceiveUpdate()
    {
        SocketReceive(server);
        SocketReceive(client);
    }

    private void SocketReceive(TcpSocket tcpSocket)
    {
        if (tcpSocket.Available == 0)
            return;
        IPEndPoint receive = Constants.ReceiveEndpointV4;
        if (tcpSocket.AvailableV6 > 0)
        {
            receive = Constants.ReceiveEndpointV6;
        }

        var rented = MemoryPool<byte>.Shared.Rent(CoreSocket.BufferSize);
        tcpSocket.Receive(rented.Memory, receive).AsTask().
            ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Information("Task failed!");
                    return;
                }
                var receiveFromResult = completedTask.Result;

                var remoteEendpoint = tcpSocket.RemoteEndPoint;

                Log.Information("Bytes {len} received from {address}", receiveFromResult.ReceivedBytes, remoteEendpoint);

                if (remoteEendpoint is not IPEndPoint endPoint)
                {
                    Log.Debug("Remote is not IPEndPoint, it is type: {type}", remoteEendpoint?.GetType());
                    return;
                }

                if (!MainProcessor.CanProcess(endPoint, out string accountId))
                    return;

                MainProcessor.ReceiveProcess(this, rented.Memory[..receiveFromResult.ReceivedBytes], endPoint, accountId);

                rented.Dispose();
            });
    }

    private void SendUpdate()
    {

    }
}

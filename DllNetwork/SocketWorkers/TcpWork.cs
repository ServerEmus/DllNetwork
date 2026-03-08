using DllNetwork.PacketProcessors;
using DllSocket;
using Serilog;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

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

        Log.Information("Connecting to {address} for account ID {accountId}", address, accountId);
        client.Connect(address.Address, address.Port);
    }

    public void Connected(Socket connectedSocket)
    {
        Log.Information("New TCP connection from {address} ({fancy})", connectedSocket.RemoteEndPoint, connectedSocket.ToFancyString());

        UserIdToSocket.Add(new() { UserId = string.Empty, Socket = connectedSocket });

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
    List<UserIdSocket> toRemove = [];
    private void ReceiveUpdate()
    {
        SocketReceive(server);
        SocketReceive(client);

        toRemove.Clear();
        foreach (var userIdSocket in UserIdToSocket)
        {
            if (userIdSocket.Socket == null)
            {
                toRemove.Add(userIdSocket);
                continue;
            }
            ConnectedSocketReceive(userIdSocket.Socket);
        }

        foreach (var item in toRemove)
        {
            UserIdToSocket.Remove(item);
        }
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

    private void ConnectedSocketReceive(Socket socket)
    {
        if (socket.Available == 0)
            return;
        IPEndPoint receive = socket.AddressFamily == AddressFamily.InterNetwork ? Constants.ReceiveEndpointV4 : Constants.ReceiveEndpointV6;
        var rented = MemoryPool<byte>.Shared.Rent(CoreSocket.BufferSize);
        socket.ReceiveFromAsync(rented.Memory, receive).AsTask().
            ContinueWith((completedTask) =>
            {
                if (!completedTask.IsCompletedSuccessfully)
                {
                    Log.Information("Task failed!");
                    return;
                }
                var receiveFromResult = completedTask.Result;

                var remoteEendpoint = socket.RemoteEndPoint;

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
        foreach (var item in PacketQueue)
        {

        }
    }
}

using Serilog;
using System.Net;

namespace DllSocket.Test;

public class BroadcastUdpTest
{
    [Fact]
    public async Task TestAll()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")))
        {
            return;
        }

        var logger = new LoggerConfiguration().WriteTo.File("logs_broadcast.txt").CreateLogger();
        Serilog.Log.Logger = logger;
        BroadcastSocket server = new();
        server.OnException += Server_OnException;
        BroadcastSocket client = new();
        client.OnException += Client_OnException;
        server.Start();
        client.Start();
        Assert.NotNull(server.socketv4);
        Assert.NotNull(client.socketv4);
        server.Bind(TestConst.AnyHost, TestConst.AnyV6Host);
        client.Bind(TestConst.AnyClient, TestConst.AnyV6Client);
        Assert.Equal(TestConst.AnyHost, server.socketv4.LocalEndPoint);
        Assert.Equal(TestConst.AnyV6Host, server.socketv6!.LocalEndPoint);

        await client.Send(TestConst.DataToSend, TestConst.ConnectV4);
        byte[] receiveData = new byte[4];
        int recevied = await server.Receive(receiveData, true);
        Assert.Equal(TestConst.DataToSend.Length, recevied);
        Assert.Equal(TestConst.DataToSend, receiveData);

        int sent = await client.Send(TestConst.DataToSend, TestConst.Broadcast);
        Assert.Equal(TestConst.DataToSend.Length, sent);
        byte[] receivedBytes = new byte[sent];
        int received = await server.Receive(receivedBytes, TestConst.Broadcast.Serialize());
        Assert.Equal(TestConst.DataToSend, receivedBytes);
        Assert.Equal(TestConst.DataToSend.Length, received);

        sent = await client.Send(TestConst.DataToSend, TestConst.Broadcast);
        Assert.Equal(TestConst.DataToSend.Length, sent);
        receivedBytes = new byte[sent];
        var receivedResult = await server.Receive(receivedBytes, TestConst.Broadcast);
        Assert.Equal(TestConst.DataToSend, receivedBytes);
        Assert.Equal(TestConst.DataToSend.Length, receivedResult.ReceivedBytes);

        server.Stop();
        Assert.Null(server.socketv4);
        Assert.Null(server.socketv6);
        client.Stop();
        Assert.Null(client.socketv4);
        Assert.Null(client.socketv6);
    }

    private void Client_OnException(Exception obj)
    {
        Log.Error("Client Exception: {ex}", obj);
    }

    private void Server_OnException(Exception obj)
    {
        Log.Error("Server Exception: {ex}", obj);
    }
}

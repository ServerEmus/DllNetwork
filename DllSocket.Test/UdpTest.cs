using Serilog;
using System.Net;

namespace DllSocket.Test;

public class UdpTest
{
    [Fact]
    public async Task TestAll()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")))
        {
            return;
        }

        var logger = new LoggerConfiguration().WriteTo.File("logs_udp.txt").CreateLogger();
        Serilog.Log.Logger = logger;
        UdpSocket server = new();
        server.OnException += Server_OnException;
        UdpSocket client = new();
        client.OnException += Client_OnException;
        server.Start();
        client.Start();
        Assert.NotNull(server.socketv4);
        Assert.NotNull(client.socketv4);
        server.Bind(TestConst.AnyHost_UPD, TestConst.AnyV6Host_UPD);
        Assert.Equal(TestConst.AnyHost_UPD, server.socketv4!.LocalEndPoint);
        Assert.Equal(TestConst.AnyV6Host_UPD, server.socketv6!.LocalEndPoint);
        Log.Information("Host v4 {host}", server.socketv4.ToFancyString());
        Log.Information("Host v6 {host}", server.socketv6.ToFancyString());
        client.Bind(TestConst.AnyClient, TestConst.AnyV6Client);
        Log.Information("Client v4 {host}", client.socketv4.ToFancyString());
        Log.Information("Client v6 {host}", client.socketv6!.ToFancyString());
        await client.Send(TestConst.DataToSend, TestConst.ConnectV4_UPD);
        byte[] receiveData = new byte[4];
        int recevied = await server.Receive(receiveData, true);
        Assert.Equal(TestConst.DataToSend.Length, recevied);
        Assert.Equal(TestConst.DataToSend, receiveData);

        await client.Send(TestConst.DataToSend, TestConst.ConnectV6_UPD);
        receiveData = new byte[4];
        recevied = await server.Receive(receiveData, false);
        Assert.Equal(TestConst.DataToSend.Length, recevied);
        Assert.Equal(TestConst.DataToSend, receiveData);

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

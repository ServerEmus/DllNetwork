using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DllSocket.Test;

public class TcpTest
{
    [Fact]
    public async Task TestAll()
    {
        var logger = new LoggerConfiguration().WriteTo.File("logs_tcp.txt").CreateLogger();
        Serilog.Log.Logger = logger;
        TcpSocket server = new();
        server.OnException += Server_OnException;
        TcpSocket client = new();
        client.OnException += Client_OnException;
        server.Start();
        client.Start();
        Assert.NotNull(server.socketv4);
        Assert.NotNull(client.socketv4);
        server.Bind(TestConst.AnyHost_TCP, TestConst.AnyV6Host_TCP);
        Assert.Equal(TestConst.AnyHost_TCP, server.socketv4.LocalEndPoint);
        Assert.Equal(TestConst.AnyV6Host_TCP, server.socketv6!.LocalEndPoint);
        client.Connect(TestConst.ConnectV4_TCP.Address, TestConst.ConnectV4_TCP.Port);
        Log.Information("Host v4 {host}", server.socketv4.ToFancyString());
        Log.Information("Host v6 {host}", server.socketv6.ToFancyString());
        server.Update();
        client.Update();
        Assert.Single(server.AcceptedSockets);
        Socket acceptedClient = server.AcceptedSockets[0];
        Log.Information("Client {host}", acceptedClient.ToFancyString());
        Assert.NotNull(acceptedClient);
        int sentData = await server.Send(acceptedClient, TestConst.DataToSend);
        Assert.Equal(TestConst.DataToSend.Length, sentData);
        byte[] receiveBytes = new byte[sentData];
        int received =  await client.Receive(receiveBytes, TestConst.AnyHost_TCP.Serialize());
        Assert.Equal(TestConst.DataToSend.Length, received);
        Assert.Equal(TestConst.DataToSend, receiveBytes);
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

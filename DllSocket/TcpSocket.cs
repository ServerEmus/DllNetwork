using System.Net;
using System.Net.Sockets;

namespace DllSocket;

public class TcpSocket(bool enableIpv6 = true) : CoreSocket(SocketType.Stream, ProtocolType.Tcp, enableIpv6)
{
    private bool isListening;
    public event Action<Socket>? OnAccepted;
    public readonly List<Socket> AcceptedSockets = [];

    public EndPoint? LocalEndPoint
    {
        get
        {
            if (EnableIpv6 && socketv6 != null && socketv6.IsBound)
                return socketv6.LocalEndPoint;

            if (socketv4 != null && socketv4.IsBound)
                return socketv4.LocalEndPoint;

            return null;
        }
    }

    public EndPoint? RemoteEndPoint
    {
        get
        {
            if (EnableIpv6 && socketv6 != null && socketv6.Connected)
                return socketv6.RemoteEndPoint;

            if (socketv4 != null && socketv4.Connected)
                return socketv4.RemoteEndPoint;

            return null;
        }
    }

    public void Connect(IPAddress address, int port)
    {
        if (socketv4 == null)
            return;

        try
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                socketv4.BeginConnect(address, port, socketv4.EndConnect, socketv4);

            if (EnableIpv6 && socketv6 != null && address.AddressFamily == AddressFamily.InterNetworkV6)
                socketv6.BeginConnect(address, port, socketv6.EndConnect, socketv6);
        }
        catch (Exception ex)
        {
            InvokeException(ex);
        }
    }

    public void Disconnect()
    {
        if (socketv4 == null)
            return;

        try
        {
            if (socketv4.Connected)
                socketv4.BeginDisconnect(true, socketv4.EndDisconnect, socketv4);

            if (EnableIpv6 && socketv6 != null && socketv6.Connected)
                socketv6.BeginDisconnect(true, socketv6.EndDisconnect, socketv6);

            foreach (var acceptedSocket in AcceptedSockets)
            {
                if (acceptedSocket.Connected)
                    acceptedSocket.BeginDisconnect(true, acceptedSocket.EndDisconnect, acceptedSocket);
            }
        }
        catch (Exception ex)
        {
            InvokeException(ex);
        }
    }

    protected override void OnSocketBind()
    {
        if (socketv4 == null)
            return;
        
        socketv4.Listen();

        if (EnableIpv6 && socketv6 != null)
            socketv6.Listen();

        isListening = true;
    }

    public override void Update()
    {
        if (!isListening)
            return;

        if (socketv4 == null)
            return;

        try
        {
            socketv4.BeginAccept(ar =>
            {
                try
                {
                    Socket acceptedSocket = socketv4.EndAccept(ar);
                    if (acceptedSocket == null) return;
                    OnSocketAdded(acceptedSocket);
                }
                catch (SocketException socketException)
                {
                    if (socketException.SocketErrorCode == SocketError.OperationAborted)
                        return;
                    InvokeException(socketException);
                }
            }, socketv4);

            if (EnableIpv6 && socketv6 != null)
            {
                socketv6.BeginAccept(ar =>
                {
                    try
                    {
                        Socket acceptedSocket = socketv6.EndAccept(ar);
                        if (acceptedSocket == null) return;
                        OnSocketAdded(acceptedSocket);
                    }
                    catch (SocketException socketException)
                    {
                        if (socketException.SocketErrorCode == SocketError.OperationAborted)
                            return;
                        InvokeException(socketException);
                    }
                }, socketv6);
            }
        }
        catch (SocketException ex)
        {
            InvokeException(ex);
        }

        base.Update();
    }

    protected virtual void OnSocketAdded(Socket socket)
    {
        socket.Blocking = false;
        socket.NoDelay = true;
        AcceptedSockets.Add(socket);
        OnAccepted?.Invoke(socket);
    }

    public ValueTask<int> Send(Socket socket, ReadOnlyMemory<byte> data, SocketFlags flags = SocketFlags.None)
    {
        if (socket == null)
            return ValueTask.FromResult(-1);

        try
        {
            return socket.SendAsync(data, flags);
        }
        catch (SocketException ex)
        {
            InvokeException(ex);
        }

        return ValueTask.FromResult(-1);
    }
}

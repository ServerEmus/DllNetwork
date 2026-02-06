using System.Net;
using System.Net.Sockets;

namespace DllSocket;

public class CoreSocket(SocketType socketType, ProtocolType protocolType, bool enableIpv6 = true)
{
    public const int BufferSize = 1024 * 1024;
    public event Action<Exception>? OnException;
    public event Action? OnStarted;

    public bool EnableIpv6 = enableIpv6;
    public readonly SocketType SocketType = socketType;
    public readonly ProtocolType ProtocolType = protocolType;
    public Socket? socketv4;
    public Socket? socketv6;

    public int Available
    {
        get
        {
            if (EnableIpv6 && socketv6 != null)
                return socketv6.Available;

            if (socketv4 != null)
                return socketv4.Available;

            return 0;
        }
    }

    public int AvailableV4
    {
        get
        {
            if (socketv4 != null)
                return socketv4.Available;

            return 0;
        }
    }

    public int AvailableV6
    {
        get
        {
            if (EnableIpv6 && socketv6 != null)
                return socketv6.Available;

            return 0;
        }
    }

    public void Start()
    {
        try
        {
            socketv4 = new(AddressFamily.InterNetwork, SocketType, ProtocolType)
            {
                Blocking = false,
                ReceiveBufferSize = BufferSize,
                SendBufferSize = BufferSize,
            };

            if (EnableIpv6)
            {
                socketv6 = new(AddressFamily.InterNetworkV6, SocketType, ProtocolType)
                {
                    Blocking = false,
                    ReceiveBufferSize = BufferSize,
                    SendBufferSize = BufferSize,
                };
            }

            OnSocketStarted();
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }
    }

    public void Bind(EndPoint endPointv4, EndPoint? endPointv6)
    {
        if (endPointv4 == null)
            return;

        if (socketv4 == null)
            return;

        try
        {
            socketv4.Bind(endPointv4);

            if (EnableIpv6 && socketv6 != null && endPointv6 != null)
            {
                socketv6.Bind(endPointv6);
            }

            OnSocketBind();
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }
    }

    public ValueTask<int> Send(ReadOnlyMemory<byte> data, EndPoint endPoint, SocketFlags flags = SocketFlags.None)
    {
        return Send(data, endPoint.Serialize(), flags);
    }

    public ValueTask<int> Send(ReadOnlyMemory<byte> data, SocketAddress address, SocketFlags flags = SocketFlags.None)
    {
        if (address == null)
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        if (socketv4 == null)
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        if (data.IsEmpty)
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        try
        {
            if (address.Family == AddressFamily.InterNetwork)
                return socketv4.SendToAsync(data, flags, address);

            if (EnableIpv6 && socketv6 != null && address.Family == AddressFamily.InterNetworkV6)
                return socketv6.SendToAsync(data, flags, address);
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }

        return ValueTask.FromCanceled<int>(CancellationToken.None);
    }

    public ValueTask<SocketReceiveFromResult> Receive(Memory<byte> data, EndPoint endPoint, SocketFlags flags = SocketFlags.None)
    {
        if (socketv4 == null)
            return ValueTask.FromCanceled<SocketReceiveFromResult>(CancellationToken.None);

        if (data.IsEmpty)
            return ValueTask.FromCanceled<SocketReceiveFromResult>(CancellationToken.None);

        try
        {
            if (endPoint.AddressFamily == AddressFamily.InterNetwork)
                return socketv4.ReceiveFromAsync(data, flags, endPoint);

            if (EnableIpv6 && socketv6 != null && endPoint.AddressFamily == AddressFamily.InterNetworkV6)
                return socketv6.ReceiveFromAsync(data, flags, endPoint);
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }
        catch (System.Security.SecurityException ex)
        {
            OnException?.Invoke(ex);
        }

        return ValueTask.FromCanceled<SocketReceiveFromResult>(CancellationToken.None);
    }

    public ValueTask<int> Receive(Memory<byte> data, SocketAddress address, SocketFlags flags = SocketFlags.None)
    {
        if (socketv4 == null) 
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        if (data.IsEmpty) 
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        try
        {
            if (address.Family == AddressFamily.InterNetwork)
                return socketv4.ReceiveFromAsync(data, flags, address);

            if (EnableIpv6 && socketv6 != null && address.Family == AddressFamily.InterNetworkV6)
                return socketv6.ReceiveFromAsync(data, flags, address);
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }

        return ValueTask.FromCanceled<int>(CancellationToken.None);
    }

    public ValueTask<int> Receive(Memory<byte> data, bool useIpv4 = true)
    {
        if (socketv4 == null)
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        if (data.IsEmpty)
            return ValueTask.FromCanceled<int>(CancellationToken.None);

        try
        {
            if (useIpv4)
                return socketv4.ReceiveAsync(data);

            if (EnableIpv6 && socketv6 != null)
                return socketv6.ReceiveAsync(data);  
        }
        catch (SocketException ex)
        {
            OnException?.Invoke(ex);
        }

        return ValueTask.FromCanceled<int>(CancellationToken.None);
    }

    public void Stop()
    {
        socketv4?.Dispose();
        socketv4 = null;
        socketv6?.Dispose();
        socketv6 = null;
    }

    public virtual void Update() 
    {
        OnUpdate();
    }

    protected void InvokeException(Exception ex)
    {
        OnException?.Invoke(ex);
    }

    protected virtual void OnUpdate() { }

    protected virtual void OnSocketStarted()
    {
        OnStarted?.Invoke();
    }

    protected virtual void OnSocketBind() { }
}

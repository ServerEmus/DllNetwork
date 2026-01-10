namespace DllSocket;

public class BroadcastSocket(bool enableIpv6 = true) : UdpSocket(enableIpv6)
{
    protected override void OnSocketStarted()
    {
        if (socketv4 == null) return;
        socketv4.EnableBroadcast = true;

        if (EnableIpv6 && socketv6 != null)
            socketv6.EnableBroadcast = true;
    }
}


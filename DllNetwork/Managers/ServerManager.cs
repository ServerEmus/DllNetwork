using LiteNetLib;
using LiteNetLib.Utils;
using DllNetwork.Listeners;

namespace DllNetwork.Managers;

public class ServerManager
{
    private readonly NetManager _manager;
    private readonly NetDataWriter writer = new();

    public static ServerManager Instance
    {
        get
        {
            field ??= new();
            return field;
        }
    }

    public bool IsRunning => _manager.IsRunning;

    public ServerManager()
    {
        _manager = new(ServerListener.Listener.Value)
        {
            BroadcastReceiveEnabled = false,
            DontRoute = true,
            IPv6Enabled = NetworkSettings.Instance.Manager.EnableIpv6,
            UnconnectedMessagesEnabled = false,
            UnsyncedDeliveryEvent = true,
            UnsyncedEvents = true,
            UnsyncedReceiveEvent = true,
            ChannelsCount = 32,
        };
    }

    public int Port => _manager.LocalPort;

    public bool Start(int port = 0)
    {
        return _manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, port);
    }

    public void Update()
    {
        _manager.TriggerUpdate();
    }

    public void Stop()
    {
        _manager.Stop();
    }

    public void Send<T>(T data, string? accountId = null, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered) where T : INetSerializable
    {
        writer.Reset();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref data);
        if (string.IsNullOrEmpty(accountId))
        {
            _manager.SendToAll(writer, channelNumber, options);
            return;
        }

        if (!NetPeerStore.TryGetPeerId(accountId, out int peerId))
            return;

        NetPeer peer = (NetPeer)_manager.GetPeerById(peerId);
        peer.Send(writer, channelNumber, options);
    }

    public void Send(ReadOnlySpan<byte> data, string? accountId = null, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            _manager.SendToAll(data.ToArray(), channelNumber, options);
            return;
        }

        if (!NetPeerStore.TryGetPeerId(accountId, out int peerId))
            return;

        NetPeer peer = (NetPeer)_manager.GetPeerById(peerId);
        peer.Send(data, channelNumber, options);
    }
}

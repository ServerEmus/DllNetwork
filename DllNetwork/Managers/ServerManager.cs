using DllNetwork.Listeners;
using DllNetwork.Settings;
using LiteNetLib;
using LiteNetLib.Utils;

namespace DllNetwork.Managers;

public class ServerManager
{
    public static ServerManager Instance
    {
        get
        {
            field ??= new();
            return field;
        }
    }

    private readonly NetManager manager;
    private readonly NetDataWriter writer = new();

    public ServerManager()
    {
        manager = new(ServerListener.Listener.Value)
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

    public bool IsRunning => manager.IsRunning;

    public int Port => manager.LocalPort;

    public bool Start(int port = 0)
    {
        return manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, port);
    }

    public void Update()
    {
        manager.TriggerUpdate();
    }

    public void Stop()
    {
        manager.Stop();
    }

    public void Send<T>(T data, string? accountId = null, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
        where T : INetSerializable
    {
        writer.Reset();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref data);
        if (string.IsNullOrEmpty(accountId))
        {
            manager.SendToAll(writer, channelNumber, options);
            return;
        }

        if (!AccountStorage.TryGetPeerId(accountId, out int peerId))
        {
            return;
        }

        NetPeer peer = (NetPeer)manager.GetPeerById(peerId);
        peer.Send(writer, channelNumber, options);
    }

    public void Send(ReadOnlySpan<byte> data, string? accountId = null, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            manager.SendToAll(data.ToArray(), channelNumber, options);
            return;
        }

        if (!AccountStorage.TryGetPeerId(accountId, out int peerId))
        {
            return;
        }

        NetPeer peer = (NetPeer)manager.GetPeerById(peerId);
        peer.Send(data, channelNumber, options);
    }
}

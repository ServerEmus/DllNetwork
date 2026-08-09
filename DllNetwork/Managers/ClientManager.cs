using DllNetwork.Listeners;
using DllNetwork.Settings;
using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System.Net;

namespace DllNetwork.Managers;

public class ClientManager
{
    public static ClientManager Instance
    {
        get
        {
            field ??= new();
            return field;
        }
    }

    private readonly NetManager manager;
    private readonly NetDataWriter writer = new();
    private readonly Dictionary<string, NetPeer?> accountToPeer = [];

    public ClientManager()
    {
        manager = new(ClientListener.Listener.Value)
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

        ClientListener.OnDisconnected += ClientListener_OnDisconnected;
    }

    public bool IsRunning => manager.IsRunning;

    public void Start()
    {
        manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, 0);
        Log.Information("[NetClient.Start] Started on {Port}", manager.LocalPort);
    }

    public void Connect(string address, int port, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        accountToPeer[accountId] = manager.Connect(address, port, writer);
    }

    public void Connect(IPEndPoint endPoint, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        accountToPeer[accountId] = manager.Connect(endPoint, writer);
    }

    public void Update()
    {
        manager.TriggerUpdate();
    }

    public void Disconnect(string accountId)
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            return;
        }

        manager.DisconnectPeer(peer);
    }

    public void Stop()
    {
        manager.Stop();
    }

    public void Send<T>(T data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
        where T : INetSerializable
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            return;
        }

        writer.Reset();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref data);
        peer.Send(writer, channelNumber, options);
    }

    public void Send(ReadOnlySpan<byte> data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            return;
        }

        peer.Send(data, channelNumber, options);
    }

    public bool IsAccountConnected(string accountId)
    {
        return accountToPeer.ContainsKey(accountId);
    }

    private void ClientListener_OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        for (int i = 0; i < accountToPeer.Count; i++)
        {
            var kv = accountToPeer.ElementAt(i);
            if (kv.Value == peer)
            {
                accountToPeer.Remove(kv.Key);
            }
        }
    }
}

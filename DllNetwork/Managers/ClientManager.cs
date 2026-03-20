using LiteNetLib;
using LiteNetLib.Utils;
using DllNetwork.Listeners;
using Serilog;
using System.Net;

namespace DllNetwork.Managers;

public class ClientManager
{
    private readonly NetManager _manager;
    private readonly NetDataWriter writer = new();
    private readonly Dictionary<string, NetPeer?> AccountToPeer = [];
    public static ClientManager Instance
    {
        get
        {
            field ??= new();
            return field;
        }
    }

    public bool IsRunning => _manager.IsRunning;

    public ClientManager()
    {
        _manager = new(ClientListener.Listener.Value)
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

    public void Start()
    {
        
        _manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, 0);
        Log.Information("[NetClient.Start] Started on {Port}", _manager.LocalPort);
    }

    public void Connect(string address, int port, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        AccountToPeer[accountId] = _manager.Connect(address, port, writer);
    }

    public void Connect(IPEndPoint endPoint, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        AccountToPeer[accountId] = _manager.Connect(endPoint, writer);
    }

    public void Update()
    {
        _manager.TriggerUpdate();
    }

    public void Disconnect(string accountId)
    {
        if (!AccountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
            return;

        _manager.DisconnectPeer(peer);
    }

    public void Stop()
    {
        _manager.Stop();
    }

    public void Send<T>(T data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered) where T : INetSerializable
    {
        if (!AccountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
            return;

        writer.Reset();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref data);
        peer.Send(writer, channelNumber, options);
    }

    public void Send(ReadOnlySpan<byte> data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (!AccountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
            return;

        peer.Send(data, channelNumber, options);
    }

    public bool IsAccountConnected(string accountId)
    {
        return AccountToPeer.ContainsKey(accountId);
    }

    private void ClientListener_OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        for (int i = 0; i < AccountToPeer.Count; i++)
        {
            var kv = AccountToPeer.ElementAt(i);
            if (kv.Value == peer)
                AccountToPeer.Remove(kv.Key);
        }
    }
}

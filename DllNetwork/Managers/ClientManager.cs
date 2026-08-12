using DllNetwork.Json;
using DllNetwork.Listeners;
using DllNetwork.Settings;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace DllNetwork.Managers;

/// <summary>
/// Represent a client network.
/// </summary>
public class ClientManager
{
    /// <summary>
    /// Called when accountId is failed to connect.
    /// </summary>
    public static event Action<string, IPAddress>? OnConnectionFailed;

    /// <summary>
    /// Gets the client network instance.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientManager"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the client is runnning.
    /// </summary>
    public bool IsRunning => manager.IsRunning;

    /// <summary>
    /// Starts the client connection.
    /// </summary>
    public void Start()
    {
        manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, 0);
        NetworkLog.Logger.Information("[NetClient.Start] Started on {Port}", manager.LocalPort);
    }

    /// <summary>
    /// Connect to remote host.
    /// </summary>
    /// <param name="address">Server IP or hostname.</param>
    /// <param name="port">Server port.</param>
    /// <param name="accountId">Account Id to connect.</param>
    public void Connect(string address, int port, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        accountToPeer[accountId] = manager.Connect(address, port, writer);
        NetworkLog.Logger.Debug($"Connected: {accountId} {accountToPeer[accountId] != null} {address}");
    }

    /// <summary>
    /// Connect to remote host.
    /// </summary>
    /// <param name="endPoint">Server end point (ip and port).</param>
    /// <param name="accountId">Account Id to connect.</param>
    public void Connect(IPEndPoint endPoint, string accountId)
    {
        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        accountToPeer[accountId] = manager.Connect(endPoint, writer);
        NetworkLog.Logger.Debug($"Connected: {accountId} {accountToPeer[accountId] != null} {endPoint.Address}");
    }

    /// <summary>
    /// Connect to remote host via <paramref name="broadcastAccount"/>.
    /// </summary>
    /// <param name="broadcastAccount">The account to connect.</param>
    /// <param name="endpointIndex">The index of the address to connect with.</param>
    public void Connect(BroadcastAccount broadcastAccount, int endpointIndex = 0)
    {
        if (broadcastAccount.Addresses.Count > endpointIndex)
        {
            return;
        }

        string address = broadcastAccount.Addresses[endpointIndex];

        writer.Reset();
        writer.Put(NetworkSettings.Instance.Connection.ConnectionKey);
        writer.Put(NetworkSettings.Instance.Account.AccountId);
        accountToPeer[broadcastAccount.AccountId] = manager.Connect(address, broadcastAccount.Port, writer);
        NetworkLog.Logger.Debug($"Connected: {broadcastAccount.AccountId} {accountToPeer[broadcastAccount.AccountId] != null} {address}");
    }

    /// <summary>
    /// Runs an update logic.
    /// </summary>
    public void Update()
    {
        manager.TriggerUpdate();
    }

    /// <summary>
    /// Disconnect the connection with an account.
    /// </summary>
    /// <param name="accountId">The account to disconnect.</param>
    public void Disconnect(string accountId)
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            return;
        }

        manager.DisconnectPeer(peer);
    }

    /// <summary>
    /// Stops the client connection.
    /// </summary>
    public void Stop()
    {
        manager.Stop();
    }

    /// <summary>
    /// Send a packet to the destination.
    /// </summary>
    /// <typeparam name="T">Any <see cref="INetSerializable"/>.</typeparam>
    /// <param name="data">The packed data.</param>
    /// <param name="accountId">The account Id to send to.</param>
    /// <param name="channelNumber">The channel number to use.</param>
    /// <param name="options">The method to deliver.</param>
    public void Send<T>(T data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
        where T : INetSerializable
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            NetworkLog.Logger.Warning("[ClientManager.Send] AccountId ({AccountId}) not found", accountId);
            return;
        }

        writer.Reset();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref data);
        peer.Send(writer, channelNumber, options);
    }

    /// <summary>
    /// Sends a data to the destination.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="accountId">The account Id to send to.</param>
    /// <param name="channelNumber">The channel number to use.</param>
    /// <param name="options">The method to deliver.</param>
    public void Send(ReadOnlySpan<byte> data, string accountId, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (!accountToPeer.TryGetValue(accountId, out NetPeer? peer) || peer == null)
        {
            NetworkLog.Logger.Warning("[ClientManager.Send] AccountId ({AccountId}) not found", accountId);
            return;
        }

        peer.Send(data, channelNumber, options);
    }

    /// <summary>
    /// Checks whenever the <paramref name="accountId"/> is connected.
    /// </summary>
    /// <param name="accountId">The account Id to check.</param>
    /// <returns><see langword="true"/> if account is connected; otherwise, <see langword="false"/>.</returns>
    public bool IsAccountConnected(string accountId)
    {
        return accountToPeer.ContainsKey(accountId);
    }

    /// <summary>
    /// Connect to self created server.
    /// </summary>
    public void SelfConnect()
    {
        int port = ServerManager.Instance.Port;
        if (port == 0)
        {
            NetworkLog.Logger.Warning("Server port is 0! Cannot connect to self!");
            return;
        }

        string connectIp = NetworkSettings.Instance.Binding.BindIpv4;

        if (connectIp == "0.0.0.0")
        {
            connectIp = IPAddress.Loopback.ToString();
        }

        Connect(connectIp, port, NetworkSettings.Instance.Account.AccountId);
    }

    private void ClientListener_OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        for (int i = 0; i < accountToPeer.Count; i++)
        {
            var kv = accountToPeer.ElementAt(i);
            if (kv.Value != peer)
            {
                continue;
            }

            string accountId = kv.Key;

            accountToPeer.Remove(accountId);
            OnConnectionFailed?.Invoke(accountId, peer.Address);
        }
    }
}

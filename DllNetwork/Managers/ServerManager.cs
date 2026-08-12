using DllNetwork.Listeners;
using DllNetwork.Settings;
using LiteNetLib;
using LiteNetLib.Utils;

namespace DllNetwork.Managers;

/// <summary>
/// Represent a server network.
/// </summary>
public class ServerManager
{
    /// <summary>
    /// Gets the server network instance.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerManager"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the client is runnning.
    /// </summary>
    public bool IsRunning => manager.IsRunning;

    /// <summary>
    /// Gets the port the server running on.
    /// </summary>
    public int Port => manager.LocalPort;

    /// <summary>
    /// Starts the client connection.
    /// </summary>
    /// <param name="port">The port to listen.</param>
    /// <returns><see langword="true"/> if server started; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// When <paramref name="port"/> is left empty it automaticly finds a usable port.
    /// </remarks>
    public bool Start(int port = 0)
    {
        return manager.Start(NetworkSettings.Instance.Binding.BindIpv4, NetworkSettings.Instance.Binding.BindIpv6, port);
    }

    /// <summary>
    /// Runs an update logic.
    /// </summary>
    public void Update()
    {
        manager.TriggerUpdate();
    }

    /// <summary>
    /// Stops the server connection.
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
    /// <remarks>
    /// When <paramref name="accountId"/> is empty it sends to all connection.
    /// </remarks>
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
            NetworkLog.Logger.Warning("[ServerManager.Send] AccountId ({AccountId}) not found", accountId);
            return;
        }

        NetPeer peer = (NetPeer)manager.GetPeerById(peerId);
        peer.Send(writer, channelNumber, options);
    }

    /// <summary>
    /// Sends a data to the destination.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="accountId">The account Id to send to.</param>
    /// <param name="channelNumber">The channel number to use.</param>
    /// <param name="options">The method to deliver.</param>
    /// <remarks>
    /// When <paramref name="accountId"/> is empty it sends to all connection.
    /// </remarks>
    public void Send(ReadOnlySpan<byte> data, string? accountId = null, byte channelNumber = 0, DeliveryMethod options = DeliveryMethod.ReliableOrdered)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            manager.SendToAll(data.ToArray(), channelNumber, options);
            return;
        }

        if (!AccountStorage.TryGetPeerId(accountId, out int peerId))
        {
            NetworkLog.Logger.Warning("[ServerManager.Send] AccountId ({AccountId}) not found", accountId);
            return;
        }

        NetPeer peer = (NetPeer)manager.GetPeerById(peerId);
        peer.Send(data, channelNumber, options);
    }
}

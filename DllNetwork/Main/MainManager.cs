using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System.Net;

namespace DllNetwork.Main;

public class MainManager : NetManager
{
    public string AccountId { get; set; } = Guid.NewGuid().ToString();
    public static MainManager Instance { get; } = new();
    public MainManager() : base(MainListener.Listener)
    {
        UnconnectedMessagesEnabled = true;
        IPv6Enabled = NetworkSettings.Instance.Manager.EnableIpv6;
        ChannelsCount = NetworkSettings.Instance.Manager.ChannelCount;
        EnableStatistics = NetworkSettings.Instance.Manager.UseStatistics;
    }

    public new void Start()
    {
        IPAddress ipv4Address = IPAddress.Parse(NetworkSettings.Instance.Binding.BindIpv4);
        IPAddress ipv6Address = IPAddress.Parse(NetworkSettings.Instance.Binding.BindIpv6);
        if (!Start(ipv4Address, ipv6Address, 0))
        {
            Log.Error("Network start failed!");
            return;
        }
        Log.Debug("Network started on {Port}!", LocalPort);
    }

    public void Update()
    {
        if (!IsRunning)
            return;
        PollEvents();
    }


    public void SendBroadcast()
    {
        if (!IsRunning)
            return;

        DiscoveryRequestPacket discovery = new()
        {
            AccountId = AccountId,
        };
        NetDataWriter writer = new();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref discovery);
        SendBroadcast(writer, NetworkSettings.Instance.Broadcast.BroadcastPort);
        Log.Debug("Sent broadcast! MY account: {accountId}", AccountId);
    }

    public static void SendToAccount<TPacket>(string accountId, ref TPacket packet, byte channel = 0, DeliveryMethod delivery = DeliveryMethod.ReliableOrdered) where TPacket : INetSerializable
    {
        if (!PeerAccount.TryGetAccount(accountId, out var account))
            return;

        foreach (var peer in account.MainPeers)
        {
            peer.SendSerializable(ref packet, channel, delivery);
            Log.Debug("Sent {packet} to Account: {accountId} at {endPoint}", packet, accountId, peer);
        }
    }

}

using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System.Net;

namespace DllNetwork.Main;

public class MainManager : NetManager
{
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

        DiscoveryPacket discovery = new()
        {
            AccountId = NetworkSettings.Instance.Account.AccountId,
            IsRequest = true,
            Addresses = AddressHelper.GetInteraceAddresses(),
        };
        NetDataWriter writer = new();
        PacketProcessor.Processor.WriteNetSerializable(writer, ref discovery);
        SendBroadcast(writer, NetworkSettings.Instance.Broadcast.BroadcastPort);
        foreach (int fallbackPort in NetworkSettings.Instance.Broadcast.FallbackBroadcastPorts)
        {
            Log.Debug("Sending to fallback port: {port}", fallbackPort);
            SendBroadcast(writer, fallbackPort);
        }

        Log.Debug("Sent broadcast! MY account: {accountId}", NetworkSettings.Instance.Account.AccountId);
    }


    public static void SendToAccount<TPacket>(string accountId, ref TPacket packet, byte channel = 0, DeliveryMethod delivery = DeliveryMethod.ReliableOrdered) where TPacket : INetSerializable
    {
        if (!PeerAccount.TryGetAccount(accountId, out var account))
        {
            Log.Debug("Account not found: {id}", accountId);
            return;
        }

        foreach (var item in account.EndPoints)
        {
            Log.Debug("EndPoints: {ite}", item);
        }

        foreach (var item in account.Peers)
        {
            Log.Debug("Peers: {ite}", item);
        }

        foreach (var peer in account.MainPeers)
        {
            peer.SendSerializable(ref packet, channel, delivery);
            Log.Debug("Sent {packet} to Account: {accountId} at {endPoint}", packet, accountId, peer);
        }
    }

}

using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;

namespace DllNetwork.Broadcast;

public class BroadcastManager : NetManager
{
    public static BroadcastManager Instance { get; } = new();
    public BroadcastManager() : base(BroadcastListener.Listener)
    {
        BroadcastReceiveEnabled = true;
        IPv6Enabled = NetworkSettings.Instance.Manager.EnableIpv6;
        ChannelsCount = NetworkSettings.Instance.Manager.ChannelCount;
        EnableStatistics = NetworkSettings.Instance.Manager.UseStatistics;
        BroadcastListener.Listener.PeerConnected += Listener_PeerConnected;
        BroadcastListener.Listener.PeerDisconnected += Listener_PeerDisconnected;
    }


    private void Listener_PeerConnected(NetPeer connectedPeer)
    {
        if (!PeerAccount.TryGetAccountId(connectedPeer, out string? userid))
            return;

        Log.Debug("[Broadcast] Peer connected {peer} as {UserId}.", connectedPeer.Details, userid);
        foreach (NetPeer? peer in this)
        {
            if (peer == null)
                continue;

            if (peer == connectedPeer)
                continue;

            if (!PeerAccount.TryGetAccountId(peer, out string? peerUserId))
                continue;

            UserConnectedPacket newUserConnectedPacket = new()
            {
                AccountId = peerUserId,
                IP = peer.ToString()
            };
            connectedPeer.SendSerializable(ref newUserConnectedPacket);

            newUserConnectedPacket = new()
            {
                AccountId = userid,
                IP = connectedPeer.ToString()
            };
            peer.SendSerializable(ref newUserConnectedPacket);
        }
    }

    private void Listener_PeerDisconnected(NetPeer disconnectedPeer, DisconnectInfo disconnectInfo)
    {
        if (!PeerAccount.TryGetAccountId(disconnectedPeer, out string? userid))
            return;

        Log.Debug("[Broadcast] Peer disconnected {peer} as {UserId}.", disconnectedPeer.Details, userid);
        foreach (NetPeer? peer in this)
        {
            if (peer == null)
                continue;

            if (peer == disconnectedPeer)
                continue;

            UserDisconnectedPacket disconnectedPacket = new()
            {
                AccountId = userid,
                IP = disconnectedPeer.ToString()
            };
            peer.SendSerializable(ref disconnectedPacket);
        }
    }

    
    public new void Start()
    {
        if (Start(NetworkSettings.Instance.Broadcast.BroadcastPort))
        {
            Log.Debug("[Broadcast] Started!");
            return;
        }
        Log.Error("[Broadcast] Start failed!");
        foreach (int fallbackPort in NetworkSettings.Instance.Broadcast.FallbackBroadcastPorts)
        {
            if (Start(fallbackPort))
            {
                Log.Debug("[Broadcast] Started!");
                return;
            }    
        }
        Log.Error("[Broadcast] All fallback ports are used and still cannot start!");
    }


    public void Update()
    {
        if (!IsRunning)
            return;
        PollEvents();
    }
}

using DllNetwork.SocketWorkers;
using DllSocket;
using Serilog;
using System.Net;

namespace DllNetwork.PacketProcessors;

public static class MainProcessor
{
    public static readonly Dictionary<IPEndPoint, int> TimeoutProcessingEndpoints = [];

    public static readonly List<IPEndPoint> DenyProcessingEndpoints = [];

    public static bool CanProcess(IPEndPoint endPoint, out string accountId)
    {
        accountId = string.Empty;
        if (DenyProcessingEndpoints.Contains(endPoint))
        {
            Log.Debug("{receive} is in the Deny list, we not process anything!", endPoint);
            return false;
        }

        if (TimeoutProcessingEndpoints.TryGetValue(endPoint, out int TimeOutCheck))
        {
            Log.Debug("{receive} is in the TimeOut list, we not process anything!", endPoint);
            if (TimeOutCheck == 0)
            {
                TimeoutProcessingEndpoints.Remove(endPoint);
                return false;
            }
            TimeoutProcessingEndpoints[endPoint]--;
            return false;
        }

        if (!NetworkAccount.TryGetFromAddress(endPoint, PortType.Udp, out accountId))
        {
            Log.Warning("{receive} not found in Account storage. We no gonna process any packet from them!", endPoint);
            // TODO: Add processing queue with this endpoint (they might connect but broadcast was slower than sending message).

            return false;
        }

        if (MainNetwork.Instance.settings.Account.DenyConnectionAccounts.Contains(accountId))
        {
            Log.Debug("{Account} is in the Deny connection list, we not process anything!", accountId);
            return false;
        }

        return true;
    }

    public static void ReceiveProcess(ISocketWorker socketWorker, Memory<byte> bytes, IPEndPoint remoteEndPoint, string accountId)
    {
        Log.Debug("Received bytes in buffer: {buffer}", Convert.ToHexString(bytes.Span));
        var packet = PackExt.DeserializeNetworkPacket(bytes);
        if (packet == null)
        {
            Log.Warning("Failed to deserialize packet from {Account} {packet} {packet_type}", accountId, packet, packet?.GetType());
            return;
        }

        switch (packet)
        {
            case ConnectPacket handshakePacket:
                Log.Information("Received HandshakePacket {packet} from {Account}", handshakePacket, accountId);
                Processors.ProcessConnectPacket(socketWorker, handshakePacket, remoteEndPoint, accountId);
                break;
            case ConnectReplyPacket handshakeReplyPacket:
                Log.Information("Received HandshakeReplyPacket {packet} from {Account}", handshakeReplyPacket, accountId);
                Processors.ProcessReplyPacket(socketWorker, handshakeReplyPacket, remoteEndPoint, accountId);
                break;
            case HeartBeatPacket heartBeatPacket:
                Log.Information("Received HeartBeatPacket {packet} from {Account}", heartBeatPacket, accountId);
                Processors.ProcessHeartBeatPacket(socketWorker, heartBeatPacket, remoteEndPoint, accountId);
                break;
            default:
                Log.Information("Received {packet} from {Account}", packet, accountId);
                break;
        }
    }
}

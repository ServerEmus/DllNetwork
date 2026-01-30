using Serilog;
using System.Net;

namespace DllNetwork.PacketProcessors;

public static class MainProcessor
{
    public static readonly List<IPEndPoint> DenyProcessingEndpoints = [];

    public static bool CanProcess(IPEndPoint endPoint, out string accountId)
    {
        accountId = string.Empty;
        if (DenyProcessingEndpoints.Contains(endPoint))
        {
            Log.Debug("{receive} is in the Deny list, we not process anything!", endPoint);
            return false;
        }

        if (!NetworkAccount.TryGetFromAddress(endPoint, PortType.Udp, out accountId))
        {
            Log.Warning("{receive} not found in Account storage. We no gonna process any packet from them!", endPoint);
            return false;
        }

        if (MainNetwork.Instance.settings.Account.DenyConnectionAccounts.Contains(accountId))
        {
            Log.Debug("{Account} is in the Deny connection list, we not process anything!", accountId);
            return false;
        }

        return true;
    }

    public static void ReceiveProcess(Memory<byte> bytes, IPEndPoint remoteEndPoint, string accountId)
    {
        Log.Debug("Received bytes in buffer: {buffer}", Convert.ToHexString(bytes.ToArray()));
        var packet = PackExt.Deserialize<INetworkPacket>(bytes);
        if (packet == null)
        {
            Log.Warning("Failed to deserialize packet from {Account}", accountId);
            return;
        }

        switch (packet)
        {
            case HandshakePacket handshakePacket:
                Log.Information("Received HandshakePacket {packet} from {Account}", handshakePacket, accountId);
                HandshakePacketProcessor.ProcessPacket(handshakePacket, remoteEndPoint, accountId);
                // Process HandshakePacket
                break;
            default:
                Log.Information("Received {packet} from {Account}", packet, accountId);
                break;
        }
    }
}

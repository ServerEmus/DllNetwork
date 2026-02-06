using DllNetwork.SocketWorkers;
using Serilog;
using System.Net;

namespace DllNetwork.PacketProcessors;

public static partial class Processors
{
    public static void ProcessConnectPacket(ISocketWorker socketWorker, ConnectPacket packet, IPEndPoint endPoint, string UserId)
    {
        ConnectReplyPacket replyPacket = new();
        if (packet.Version < Constants.DLL_MIN_SUPPORTED_VERSION)
        {
            Log.Debug("Version no longer supported!");
            MainProcessor.TimeoutProcessingEndpoints.Add(endPoint, 2);
            replyPacket.DenyReason = DenyReason.VersionMissmatch;
        }

        if (packet.HandshakeKey != MainNetwork.Instance.settings.Connection.HandshakeKey)
        {
            Log.Debug("Handshake key missmatch!");
            MainProcessor.DenyProcessingEndpoints.Add(endPoint);
            replyPacket.DenyReason = DenyReason.HandshakeKeyMissmatch;
        }

        socketWorker.AddPacketQueue(replyPacket, UserId);
    }
}

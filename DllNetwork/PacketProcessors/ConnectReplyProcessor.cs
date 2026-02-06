using DllNetwork.SocketWorkers;
using Serilog;
using System.Net;

namespace DllNetwork.PacketProcessors;

public static partial class Processors
{
    public static void ProcessReplyPacket(ISocketWorker socketWorker, ConnectReplyPacket packet, IPEndPoint endPoint, string UserId)
    {
        // We got reply!
        switch (packet.DenyReason)
        {
            case DenyReason.None:
                break;
            case DenyReason.VersionMissmatch:
                Log.Warning("Version missmatch with {UserId}", UserId);
                return;
            case DenyReason.HandshakeKeyMissmatch:
                Log.Debug("Handshake Key missmatch with {UserId}", UserId);
                return;
            default:
                return;
        }

        // No deny means we can start sending heartbeats to keep the connection alive, and also we can start sending packets to the server.

        HeartBeatPacket heartBeatPacket = new();
        socketWorker.AddPacketQueue(heartBeatPacket, UserId);
    }
}

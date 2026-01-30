using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DllNetwork.PacketProcessors;

public static class HandshakePacketProcessor
{

    public static void ProcessPacket(HandshakePacket packet, IPEndPoint endPoint, string UserId)
    {
        if (packet.Version < Constants.DLL_MIN_SUPPORTED_VERSION)
        {
            Log.Warning("Version no longer supported!");
            MainProcessor.DenyProcessingEndpoints.Add(endPoint);
            // we should reply with answer packet that "hey you using non compatible version!"
        }
    }
}

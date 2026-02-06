using DllNetwork.SocketWorkers;
using Serilog;
using System.Net;

namespace DllNetwork.PacketProcessors;

public static partial class Processors
{
    public static void ProcessHeartBeatPacket(ISocketWorker socketWorker, HeartBeatPacket heartBeatPacket, IPEndPoint remoteEndPoint, string accountId)
    {
        if (socketWorker is UdpWork udpWork)
        {
            if (!UdpWork.LastHeartBeatReceived.TryGetValue(accountId, out var lastHearthBeatReceived))
            {
                UdpWork.LastHeartBeatReceived[accountId] = lastHearthBeatReceived = DateTime.UtcNow;
            }

            double timeDiff = (lastHearthBeatReceived - heartBeatPacket.SentTime).TotalSeconds;
            Log.Debug("HB debug {timediff} {time1} {time2}", timeDiff, lastHearthBeatReceived, heartBeatPacket.SentTime);
            if (timeDiff > 5)
            {
                // Send immidietly without any queueing!
                udpWork.Send(new HeartBeatPacket()
                {
                    SentTime = DateTime.UtcNow
                }, accountId);

                return;
            }

            // Add packet to queue, so it will be send in next udpWork.Update() call
            udpWork.AddPacketQueue(new HeartBeatPacket()
            {
                SentTime = DateTime.UtcNow
            }, accountId);
        }
    }
}
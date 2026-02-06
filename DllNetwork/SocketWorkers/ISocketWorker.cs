using DllSocket;

namespace DllNetwork.SocketWorkers;

public interface ISocketWorker
{
    public PortType PortType { get; }

    public CoreSocket Socket { get; }

    public void Update();

    public void AddPacketQueue(INetworkPacket networkPacket, string userId);
}

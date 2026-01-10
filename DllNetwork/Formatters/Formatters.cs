using MemoryPack;
using System.Net;

namespace DllNetwork.Formatters;

public static class Formatters
{
    public static void InitFormatters()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<INetworkPacket>())
            MemoryPackFormatterProvider.Register(INetworkPacketFormatter.Instance);

        if (!MemoryPackFormatterProvider.IsRegistered<IPAddress>())
            MemoryPackFormatterProvider.Register(IPAddressFormatter.Instance);
    }
}

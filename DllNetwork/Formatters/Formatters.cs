using EIV_Pack;
using EIV_Pack.Formatters;
using System.Net;

namespace DllNetwork.Formatters;

public static class Formatters
{
    public static void RegisterAll()
    {
        if (!FormatterProvider.IsRegistered<IPAddress>())
        {
            FormatterProvider.Register(new IPAddressFormatter());
        }

        if (!FormatterProvider.IsRegistered<List<IPAddress>>())
        {
            FormatterProvider.Register(new ListFormatter<IPAddress>());
        }

        if (!FormatterProvider.IsRegistered<INetworkPacket>())
        {
            FormatterProvider.Register(INetworkPacketFormatter.Instance);
        }

        // Registering automatically with the cctor:
        FormatterProvider.Register<AnnouncePacket>();
        FormatterProvider.Register<ConnectPacket>();
        FormatterProvider.Register<ConnectReplyPacket>();
        FormatterProvider.Register<HeartBeatPacket>();
    }
}

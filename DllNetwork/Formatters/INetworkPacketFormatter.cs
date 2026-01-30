using EIVPack;
using EIVPack.Formatters;

namespace DllNetwork.Formatters;

public class INetworkPacketFormatter : IFormatter<INetworkPacket>
{
    public delegate void GetPacketDelegate(byte PacketId, ref PackReader reader, scoped ref INetworkPacket? value);
    public delegate void WritePacketDelegate(byte PacketId, ref PackWriter writer, scoped ref readonly INetworkPacket? value);

    public event GetPacketDelegate? GetPacket;
    public event WritePacketDelegate? WritePacket;
    public static INetworkPacketFormatter Instance { get; } = new();

    public void Deserialize(ref PackReader reader, scoped ref INetworkPacket? value)
    {
        if (!reader.TryReadSmallHeader(out byte header) || header == 0)
        {
            value = null;
            return;
        }

        GetPacket?.Invoke(header, ref reader, ref value);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly INetworkPacket? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(value.PacketId);
        WritePacket?.Invoke(value.PacketId, ref writer, in value);
    }
}

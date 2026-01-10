using MemoryPack;
using System.Net;

namespace DllNetwork.Formatters;

public class IPAddressFormatter : MemoryPackFormatter<IPAddress>
{
    public static IPAddressFormatter Instance { get; } = new();
    public override void Deserialize(ref MemoryPackReader reader, scoped ref IPAddress? value)
    {
        if (!reader.TryReadObjectHeader(out byte header) || header != 1)
        {
            value = null;
            return;
        }

        byte[]? bytes = reader.ReadUnmanagedArray<byte>();
        if (bytes != null)
            value = new(bytes);
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IPAddress? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }        

        writer.WriteObjectHeader(1);
        writer.WriteUnmanagedArray(value.GetAddressBytes());
    }
}

using EIV_Pack;
using EIV_Pack.Formatters;
using System.Net;

namespace DllNetwork.Formatters;

public class IPAddressFormatter : IFormatter<IPAddress>
{
    public void Deserialize(ref PackReader reader, scoped ref IPAddress? value)
    {
        if (!reader.TryReadSmallHeader(out byte len) || len != 1)
        {
            value = null;
            return;
        }
        byte[]? data = reader.ReadArray<byte>();
        if (data == null)
        {
            value = null; 
            return;
        }
        value = new(data);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly IPAddress? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(1);
        writer.WriteArray(value.GetAddressBytes());
    }
}

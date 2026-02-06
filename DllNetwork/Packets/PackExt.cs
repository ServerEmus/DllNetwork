using DllNetwork.Formatters;
using EIV_Pack;
using System.Diagnostics.CodeAnalysis;

namespace DllNetwork.Packets;

public static class PackExt
{
    public static byte[] Serialize<T>(this T item) where T : INetworkPacket
    {
        using PackWriter packWriter = new();
        INetworkPacket networkPacket = item;
        packWriter.WriteValueWithFormatter(INetworkPacketFormatter.Instance, networkPacket);
        byte[] bytes = packWriter.GetBytes();
        packWriter.Dispose();
        return bytes;
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes) where T : INetworkPacket
    {
        PackReader reader = new(bytes);
        INetworkPacket? packet = null;
        reader.ReadValue(ref packet, INetworkPacketFormatter.Instance);
        return (T?)packet;
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes, ref T? value) where T : INetworkPacket
    {
        PackReader reader = new(bytes);
        INetworkPacket? packet = value;
        reader.ReadValue(ref packet, INetworkPacketFormatter.Instance);
        value = (T?)packet;
    }


    public static INetworkPacket? DeserializeNetworkPacket(this ReadOnlyMemory<byte> bytes) 
    {
        PackReader reader = new(bytes.Span);
        INetworkPacket? packet = null;
        reader.ReadValue(ref packet, INetworkPacketFormatter.Instance);
        return packet;
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlyMemory<byte> bytes) where T : INetworkPacket
    {
        PackReader reader = new(bytes.Span);
        INetworkPacket? packet = null;
        reader.ReadValue(ref packet, INetworkPacketFormatter.Instance);
        return (T?)packet;
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlyMemory<byte> bytes, ref T? value) where T : INetworkPacket
    {
        PackReader reader = new(bytes.Span);
        INetworkPacket? packet = value;
        reader.ReadValue(ref packet, INetworkPacketFormatter.Instance);
        value = (T?)packet;
    }
}

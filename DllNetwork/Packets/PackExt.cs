using EIV_Pack;
using System.Diagnostics.CodeAnalysis;

namespace DllNetwork.Packets;

public static class PackExt
{
    public static byte[] Serialize<T>(this T item) where T : INetworkPacket
    {
        return Serializer.Serialize(item);
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes) where T : INetworkPacket
    {
        return Serializer.Deserialize<T>(bytes);
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes, ref T? value) where T : INetworkPacket
    {
        value = Serializer.Deserialize<T>(bytes);
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlyMemory<byte> bytes) where T : INetworkPacket
    {
        return Serializer.Deserialize<T>(bytes.Span);
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlyMemory<byte> bytes, ref T? value) where T : INetworkPacket
    {
        Serializer.Deserialize(bytes.Span, ref value);
    }
}

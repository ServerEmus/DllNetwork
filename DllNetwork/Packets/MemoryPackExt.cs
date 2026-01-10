using MemoryPack;
using System.Diagnostics.CodeAnalysis;

namespace DllNetwork.Packets;

public static class MemoryPackExt
{
    public static byte[] Serialize<T>(this T item) where T : INetworkPacket
    {
        if (!MemoryPackFormatterProvider.IsRegistered<T>())
            return [];
        return MemoryPackSerializer.Serialize(item);
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes) where T : INetworkPacket
    {
        if (!MemoryPackFormatterProvider.IsRegistered<T>())
            return default;
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this byte[] bytes, ref T? value) where T : INetworkPacket
    {
        if (!MemoryPackFormatterProvider.IsRegistered<T>())
        {
            value = default;
            return;
        }
            
        MemoryPackSerializer.Deserialize(bytes, ref value);
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlySpan<byte> bytes) where T : INetworkPacket
    {
        if (!MemoryPackFormatterProvider.IsRegistered<T>())
            return default;
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }

    public static void Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ReadOnlySpan<byte> bytes, ref T? value) where T : INetworkPacket
    {
        if (!MemoryPackFormatterProvider.IsRegistered<T>())
        {
            value = default;
            return;
        }

        MemoryPackSerializer.Deserialize(bytes, ref value);
    }
}

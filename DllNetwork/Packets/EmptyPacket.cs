using LiteNetLib;
using LiteNetLib.Utils;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace DllNetwork;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represent an empty packet.
/// </summary>
public readonly struct EmptyPacket() : INetSerializable
{
    /// <summary>
    /// An always empty packet.
    /// </summary>
    public static readonly EmptyPacket Empty = new();

    /// <inheritdoc/>
    public readonly void Deserialize(NetDataReader reader)
    {
    }

    /// <inheritdoc/>
    public readonly void Serialize(NetDataWriter writer)
    {
    }
}
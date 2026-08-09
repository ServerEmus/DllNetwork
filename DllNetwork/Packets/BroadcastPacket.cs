using LiteNetLib.Utils;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace DllNetwork;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represent a packet that contains broadcast connect data.
/// </summary>
public struct BroadcastPacket : INetSerializable
{
    /// <summary>
    /// The ID of the account.
    /// </summary>
    public string Id;

    /// <summary>
    /// The IP addresses of the account.
    /// </summary>
    public string[] Addresses;

    /// <summary>
    /// The server port to connect.
    /// </summary>
    public int ConnectPort;

    /// <inheritdoc/>
    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetString();
        Addresses = reader.GetStringArray();
        ConnectPort = reader.GetInt();
    }

    /// <inheritdoc/>
    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.PutArray(Addresses);
        writer.Put(ConnectPort);
    }

    /// <inheritdoc/>
    public readonly override string ToString()
    {
        return $"{Id} {ConnectPort} {string.Join(", ", Addresses)}";
    }
}
using LiteNetLib;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace DllNetwork;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represent a sender information.
/// </summary>
public readonly struct ReceiveData
{
    /// <summary>
    /// The Peer the data send from.
    /// </summary>
    public readonly NetPeer Peer;

    /// <summary>
    /// The Id of an account the data received from.
    /// </summary>
    public readonly string AccountId;

    /// <summary>
    /// The channel the data received.
    /// </summary>
    public readonly byte Channel;

    /// <summary>
    /// The type of the received packet delivered with.
    /// </summary>
    public readonly DeliveryMethod Delivery;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiveData"/> struct.
    /// </summary>
    /// <param name="peer">The from peer.</param>
    /// <param name="channel">The received channel.</param>
    /// <param name="delivery">The type of the packet.</param>
    public ReceiveData(NetPeer peer, byte channel, DeliveryMethod delivery)
    {
        Peer = peer;
        Channel = channel;
        Delivery = delivery;

        AccountStorage.TryGetFromPeerId(peer.Id, out AccountId);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Peer} {AccountId} {Channel} {Delivery}";
    }
}

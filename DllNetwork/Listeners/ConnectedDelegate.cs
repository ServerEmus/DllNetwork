using LiteNetLib;

namespace DllNetwork.Listeners;

/// <summary>
/// Represent an action when a new peer is connected.
/// </summary>
/// <param name="peer">The new connected peer.</param>
public delegate void ConnectedDelegate(NetPeer peer);
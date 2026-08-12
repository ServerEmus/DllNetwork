using LiteNetLib;

namespace DllNetwork.Listeners;

/// <summary>
/// Represent an action when a peer is disconnected.
/// </summary>
/// <param name="peer">The disconnected peer.</param>
/// <param name="disconnectInfo">The details aboout the disconnect.</param>
public delegate void DisconnectedDelegate(NetPeer peer, DisconnectInfo disconnectInfo);

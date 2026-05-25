using LiteNetLib;

namespace DllNetwork;

public delegate void ConnectedDelegate(NetPeer peer);
public delegate void DisconnectedDelegate(NetPeer peer, DisconnectInfo disconnectInfo);
# DllNetwork


### Use of Broadcast Udp

For starting the udp clients run `BroadcastUdp.Start();`\
To actually receive new udp broadcasted accounts run `BroadcastUdp.UdpReceive();`.\
Recommended to call `BroadcastUdp.UdpReceive();` on some Update method or background thread.\
After done with everything and you not want to be broadcasted run `BroadcastUdp.Stop();`

Exammple:
```cs
// We start the udp client and send the data.
BroadcastUdp.Start();

// We tell the udp clients to starts receiving.
BroadcastUdp.UdpReceive();
// Since we stopped receiving when a new data arrieved we should call "UdpReceive" somwehere in update calls.

// We get the accounts.
var broadcastedAccs = BroadcastUdp.GetList()
    // Filter here for more than 1 addresses
    .Where(item => item.Addresses.Count > 0);

foreach (var item in broadcastedAccs)
{
    int addrCount = item.Addresses.Count;
    Console.WriteLine($"{item.AccountId} {item.Port} {addrCount}");
    // Should atleast show: (random guid) 7777 1
}

// We finally stop it
BroadcastUdp.Stop();
```

### Use of Broadcast Custom

Required to have a Broadcast Server Url set!

It is more simple then the udp one.\
To indicate we can receive networking infos simply just run `BroadcastCustom.Start();`\
When you want to get the broadcast accounts call `BroadcastCustom.GetList();`\
After done with everything and you not want to be broadcasted run `BroadcastCustom.Stop();`

Exammple:
```cs
// We connect to server.
BroadcastCustom.Start();

// We get the accounts.
var broadcastedAccs = BroadcastCustom.GetList()
    // Filter here for more than 1 addresses
    .Where(item => item.Addresses.Count > 0);

foreach (var item in broadcastedAccs)
{
    int addrCount = item.Addresses.Count;
    Console.WriteLine($"{item.AccountId} {item.Port} {addrCount}");
    // Should atleast show: (random guid) 7777 1
}

// We disconnect and finish
BroadcastCustom.Stop();
```
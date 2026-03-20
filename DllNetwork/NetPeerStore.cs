using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DllNetwork;

public class NetPeerStore
{
    private struct Store()
    {
        public string AccountId = string.Empty;
        public int PeerId;
        public int Port;
        public SortedList<long, List<IPAddress>> RTTAddresses = [];

        public List<IPAddress> NetworkAddresses
        {
            get
            {
                if (isCacheValid)
                    return cachedAddresses;

                cachedAddresses = [.. RTTAddresses.Values.SelectMany(static list => list)];
                isCacheValid = true;
                return cachedAddresses;
            }
        }

        internal bool isCacheValid = false;
        private List<IPAddress> cachedAddresses = [];
    }


    static readonly Dictionary<string, Store> Stores = [];
    public static IEnumerable<string> AccountIdList => Stores.Keys;

    public static void SetAddress(string accountId, IPAddress address, long rtt)
    {
        GetStore(accountId, out Store store);

        if (store.NetworkAddresses.Contains(address))
            return;

        store.isCacheValid = false;

        if (!store.RTTAddresses.TryGetValue(rtt, out var iPAddresses))
        {
            iPAddresses = store.RTTAddresses[rtt] = [];
        }

        iPAddresses.Add(address);

        Stores[accountId] = store;
    }

    public static void SetPort(string accountId, int port)
    {
        GetStore(accountId, out Store store);

        store.Port = port;

        Stores[accountId] = store;
    }

    public static void SetPeerId(string accountId, int peerId)
    {
        GetStore(accountId, out Store store);

        store.PeerId = peerId;

        Stores[accountId] = store;
    }

    public static void Remove(string accountId)
    {
        Stores.Remove(accountId);
    }

    public static bool TryGetPeerId(string accountId, [NotNullWhen(true)] out int peerId)
    {
        peerId = 0;
        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        peerId = store.PeerId;
        return peerId != 0;
    }

    public static bool TryGetAddress(string accountId, out List<IPAddress> addresses)
    {
        addresses = [];
        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        addresses = store.NetworkAddresses;
        return addresses.Count != 0;
    }

    public static bool TryGetFirstAddress(string accountId, [NotNullWhen(true)] out IPAddress? address)
    {
        address = null;
        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        address = store.NetworkAddresses.FirstOrDefault();
        return address != null;
    }

    public static bool TryGetPort(string accountId, out int port)
    {
        port = default;
        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        port = store.Port;
        return port != 0;
    }

    public static bool TryGetEndpoint(string accountId, [NotNullWhen(true)] out IPEndPoint? endPoint)
    {
        endPoint = null;

        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        IPAddress? ip = store.NetworkAddresses.FirstOrDefault();
        if (ip == null)
            return false;

        int port = store.Port;
        if (port == 0)
            return false;

        endPoint = new(ip, port);
        return true;
    }

    public static bool TryGetFromPeerId(int peerId, [NotNullWhen(true)] out string accountId)
    {
        accountId = string.Empty;

        foreach (var store in Stores)
        {
            if (store.Value.PeerId != peerId)
                continue;

            accountId = store.Key;
            break;
        }

        return accountId != string.Empty;
    }

    public static bool TryGetBestRTT(string accountId, out IPAddress? bestAddress)
    {
        bestAddress = null;
        if (!Stores.TryGetValue(accountId, out Store store))
            return false;

        var rttFirst = store.RTTAddresses.FirstOrDefault();
        if (rttFirst.Value.Count == 0)
            return false;

        bestAddress = rttFirst.Value.FirstOrDefault();
        return bestAddress != null;
    }

    private static void GetStore(string accountId, out Store store)
    {
        if (!Stores.TryGetValue(accountId, out store))
        {
            Stores[accountId] = store = new()
            {
                AccountId = accountId,
            };
        }
    }
}

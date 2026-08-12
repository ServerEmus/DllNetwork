using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DllNetwork;

/// <summary>
/// Provides a storage for account related information.
/// </summary>
public static class AccountStorage
{
    private static readonly Dictionary<string, Storage> Stores = [];

    /// <summary>
    /// Gets the list of account Ids.
    /// </summary>
    public static IEnumerable<string> AccountIdList => Stores.Keys;

    /// <summary>
    /// Set the <paramref name="address"/> and <paramref name="rtt"/> to the <paramref name="accountId"/>.
    /// </summary>
    /// <param name="accountId">The account Id the data belongs to.</param>
    /// <param name="address">The IP address to set.</param>
    /// <param name="rtt">The Round Trip Time to set.</param>
    public static void SetAddress(string accountId, IPAddress address, long rtt)
    {
        GetStore(accountId, out Storage store);

        if (store.NetworkAddresses.Contains(address))
        {
            return;
        }

        store.IsCacheValid = false;

        if (!store.RTTAddresses.TryGetValue(rtt, out var iPAddresses))
        {
            iPAddresses = store.RTTAddresses[rtt] = [];
        }

        iPAddresses.Add(address);

        Stores[accountId] = store;
    }

    /// <summary>
    /// Set the <paramref name="peerId"/> to the <paramref name="accountId"/>.
    /// </summary>
    /// <param name="accountId">The account Id the data belongs to.</param>
    /// <param name="peerId">The peer Id to set.</param>
    public static void SetPeerId(string accountId, int peerId)
    {
        GetStore(accountId, out Storage store);

        store.PeerId = peerId;

        Stores[accountId] = store;
    }

    /// <summary>
    /// Remove the <paramref name="accountId"/> from storage.
    /// </summary>
    /// <param name="accountId">The account to delete.</param>
    public static void Remove(string accountId)
    {
        Stores.Remove(accountId);
    }

    /// <summary>
    /// Tries to get the Peer Id with the stored <paramref name="accountId"/>.
    /// </summary>
    /// <param name="accountId">The stored accound Id.</param>
    /// <param name="peerId">The peerId or -1.</param>
    /// <returns><see langword="true"/> if account is exist and has a valid peerId; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetPeerId(string accountId, [NotNullWhen(true)] out int peerId)
    {
        peerId = -1;
        if (!Stores.TryGetValue(accountId, out Storage store))
        {
            return false;
        }

        peerId = store.PeerId;
        return peerId != -1;
    }

    /// <summary>
    /// Tries to get the IP Adddresses.
    /// </summary>
    /// <param name="accountId">The stored accound Id.</param>
    /// <param name="addresses">The valid network addresses.</param>
    /// <returns><see langword="true"/> if account is exits and has more than 0 address; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetAddress(string accountId, out List<IPAddress> addresses)
    {
        addresses = [];
        if (!Stores.TryGetValue(accountId, out Storage store))
        {
            return false;
        }

        addresses = store.NetworkAddresses;
        return addresses.Count != 0;
    }

    /// <summary>
    /// Tries to get the first IP Adddress.
    /// </summary>
    /// <param name="accountId">The stored accound Id.</param>
    /// <param name="address">The first ip adddress.</param>
    /// <returns><see langword="true"/> if account is exits and has ip address is not null; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetFirstAddress(string accountId, [NotNullWhen(true)] out IPAddress? address)
    {
        address = null;
        if (!Stores.TryGetValue(accountId, out Storage store))
        {
            return false;
        }

        address = store.NetworkAddresses.FirstOrDefault();
        return address != null;
    }

    /// <summary>
    /// Tries to get the account Id from the <paramref name="peerId"/>.
    /// </summary>
    /// <param name="peerId">The peer Id to search.</param>
    /// <param name="accountId">The stored account Id.</param>
    /// <returns><see langword="true"/> if account is exits and the peerId found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetFromPeerId(int peerId, [NotNullWhen(true)] out string accountId)
    {
        var store = Stores.FirstOrDefault(kvp => kvp.Value.PeerId == peerId);
        accountId = store.Key ?? string.Empty;

        return accountId != string.Empty;
    }

    /// <summary>
    /// Tries to get the best IP address by RTT.
    /// </summary>
    /// <param name="accountId">The stored accound Id.</param>
    /// <param name="bestAddress">The best ip address.</param>
    /// <returns><see langword="true"/> if account is exits and the ip address is not null; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetBestRTT(string accountId, out IPAddress? bestAddress)
    {
        bestAddress = null;
        if (!Stores.TryGetValue(accountId, out Storage store))
        {
            return false;
        }

        var rttFirst = store.RTTAddresses.FirstOrDefault();
        if (rttFirst.Value.Count == 0)
        {
            return false;
        }

        bestAddress = rttFirst.Value.FirstOrDefault();
        return bestAddress != null;
    }

    private static void GetStore(string accountId, out Storage store)
    {
        if (!Stores.TryGetValue(accountId, out store))
        {
            Stores[accountId] = store = new()
            {
                AccountId = accountId,
            };
        }
    }

    private struct Storage()
    {
        public string AccountId = string.Empty;
        public int PeerId;
        public readonly SortedList<long, List<IPAddress>> RTTAddresses = [];
        internal bool IsCacheValid = false;

        public List<IPAddress> NetworkAddresses
        {
            get
            {
                if (IsCacheValid)
                {
                    return field;
                }

                field = [.. RTTAddresses.Values.SelectMany(static list => list)];
                IsCacheValid = true;
                return field;
            }
        }

        = [];
    }
}

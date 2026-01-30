using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DllNetwork;

public class NetworkAccount
{
    internal static readonly Dictionary<string, NetworkAccount> Accounts = [];
    public static IEnumerable<NetworkAccount> NetAccountList => Accounts.Values;
    public static IEnumerable<string> AccountIdList => Accounts.Keys;

    public static void AddAddress(string accountId, IPAddress address, long rtt)
    {
        if (!Accounts.TryGetValue(accountId, out var account))
        {
            Accounts[accountId] = account = new()
            {
                AccountId = accountId,
            };
        }

        if (account.NetworkAddresses.Contains(address))
            return;

        account.isCacheValid = false;

        if (!account.RttToAddresses.TryGetValue(rtt, out var iPAddresses))
        {
            iPAddresses = account.RttToAddresses[rtt] = [];
        }
        
        iPAddresses.Add(address);

        Accounts[accountId] = account;
    }

    public static void AddPort(string accountId, PortStruct port)
    {
        if (!Accounts.TryGetValue(accountId, out var account))
        {
            Accounts[accountId] = account = new()
            {
                AccountId = accountId,
            };
        }

        account.Port = port;
        Accounts[accountId] = account;
    }

    public static void RemoveAccount(string accountId)
    {
        Accounts.Remove(accountId);
    }


    public static bool TryGetAccount(string accountId, [NotNullWhen(true)] out NetworkAccount? account)
    {
        return Accounts.TryGetValue(accountId, out account);
    }

    public static bool TryGetAddress(string accountId, out List<IPAddress> addresses)
    {
        addresses = [];
        if (Accounts.TryGetValue(accountId, out var account))
        {
            addresses = account.NetworkAddresses;
        }

        return addresses.Count != 0;
    }

    public static bool TryGetFirstAddress(string accountId, [NotNullWhen(true)] out IPAddress? address)
    {
        address = null;
        if (Accounts.TryGetValue(accountId, out var account))
        {
            address = account.NetworkAddresses.FirstOrDefault();
        }
        return address != null;
    }

    public static bool TryGetPort(string accountId, out PortStruct port)
    {
        port = default;
        if (Accounts.TryGetValue(accountId, out var account))
        {
            port = account.Port;
            return true;
        }
        return false;
    }

    public static bool TryGetFromAddress(IPEndPoint endPoint, PortType portType, [NotNullWhen(true)] out string accountId)
    {
        accountId = string.Empty;
        int endpointPort = endPoint.Port;
        IPAddress address = endPoint.Address;

        foreach (var account in Accounts)
        {
            int port = account.Value.Port.GetPort(portType);
            if (port != endpointPort)
                continue;

            if (!account.Value.NetworkAddresses.Contains(address))
                continue;

            accountId = account.Key;
        }

        return accountId != string.Empty;
    }

    public static bool TryGetEndpoint(string accountId, PortType portType, [NotNullWhen(true)] out IPEndPoint? endPoint)
    {
        endPoint = null;

        if (!TryGetAccount(accountId, out NetworkAccount? account))
            return false;

        IPAddress? ip = account.NetworkAddresses.FirstOrDefault();
        if (ip == null)
            return false;

        int port = account.Port.GetPort(portType);
        if (port == 0)
            return false;

        endPoint = new(ip, port);
        return true;
    }

    public string AccountId { get; private set; } = string.Empty;
    public PortStruct Port { get; private set; } = new();
    public List<IPAddress> NetworkAddresses
    {
        get
        {
            if (isCacheValid)
                return cachedAddresses;

            cachedAddresses = [.. RttToAddresses.Values.SelectMany(static list => list)];
            isCacheValid = true;
            return cachedAddresses;
        }
    }

    public SortedList<long, List<IPAddress>> RttToAddresses { get; } = [];

    private List<IPAddress> cachedAddresses = [];
    private bool isCacheValid = false;
}
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace DllNetwork;

/// <summary>
/// Provides a helper class for pinging addresses.
/// </summary>
public static class PingHelper
{
    /// <summary>
    /// A thread-safe collection for collecting IP address to RoundTripTime.
    /// </summary>
    public static readonly ConcurrentDictionary<IPAddress, long> IpToRTT = [];
    private static readonly ConcurrentDictionary<string, List<IPAddress>> AccountIdToPingedIPs = [];

    /// <summary>
    /// Sends a ping packet to <paramref name="address"/>.
    /// </summary>
    /// <param name="accountId">The account Id to ping with.</param>
    /// <param name="address">The address to ping.</param>
    /// <param name="onSuccess">The action to run when the ping success.</param>
    public static async void PingAddress(string accountId, IPAddress address, Action<string, IPAddress, long>? onSuccess = null)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            return;
        }

        if (address == null)
        {
            return;
        }

        List<IPAddress> addresses = AccountIdToPingedIPs.GetOrAdd(accountId, []);
        if (!addresses.Contains(address))
        {
            AccountIdToPingedIPs.TryUpdate(accountId, [.. addresses, address], addresses);
            NetworkLog.Logger.Information("PING {Account} -> {ip}", accountId, address);
            using Ping netPing = new();
            try
            {
                PingReply result = await netPing.SendPingAsync(address, 1000);
                if (result.Status != IPStatus.Success)
                {
                    NetworkLog.Logger.Information("PING {Account} <- {Address} {Status}", accountId, address, result.Status);
                    return;
                }

                NetworkLog.Logger.Information("PING {Account} <- {Address} {RTT} {Status}", accountId, result.Address, result.RoundtripTime, result.Status);
                IpToRTT.AddOrUpdate(result.Address, (ip) => result.RoundtripTime, (ip, rtt) => result.RoundtripTime);
                onSuccess?.Invoke(accountId, address, result.RoundtripTime);
            }
            catch (Exception ex)
            {
                NetworkLog.Logger.Error("PING {Account} with {address} was not success! {err}", accountId, address, ex);
            }
        }
    }

    /// <summary>
    /// Clears the pinged accounts with the <paramref name="accountId"/>.
    /// </summary>
    /// <param name="accountId">The account Id to clear.</param>
    public static void ClearPingedAccount(string accountId)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            return;
        }

        AccountIdToPingedIPs.TryRemove(accountId, out _);
    }

    /// <summary>
    /// Clears a specific ip address from the pinged accounts.
    /// </summary>
    /// <param name="accountId">The accoun Id to clear with.</param>
    /// <param name="address">The ip address to clear.</param>
    public static void ClearPingedAddress(string accountId, IPAddress address)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            return;
        }

        if (address == null)
        {
            return;
        }

        if (!AccountIdToPingedIPs.TryGetValue(accountId, out var addresses))
        {
            return;
        }

        if (!addresses.Contains(address))
        {
            return;
        }

        List<IPAddress> withoutIP = addresses;
        withoutIP.Remove(address);
        AccountIdToPingedIPs.TryUpdate(accountId, withoutIP, addresses);
    }
}

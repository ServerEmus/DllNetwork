using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace DllNetwork;

public static class PingHelper
{
    public static readonly ConcurrentDictionary<IPAddress, long> IpToRTT = [];
    private static readonly ConcurrentDictionary<string, List<IPAddress>> AccToPingedIPS = [];

    public static async Task PingAddress(string accountId, IPAddress address, Action<string, IPAddress, long>? onSuccess = null)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            return;
        }

        if (address == null)
        {
            return;
        }

        List<IPAddress> addresses = AccToPingedIPS.GetOrAdd(accountId, []);
        if (!addresses.Contains(address))
        {
            AccToPingedIPS.TryUpdate(accountId, [.. addresses, address], addresses);
            Log.Information("PING {Account} -> {ip}", accountId, address);
            using Ping netPing = new();
            try
            {
                PingReply result = await netPing.SendPingAsync(address, 1000);
                if (result.Status != IPStatus.Success)
                {
                    Log.Information("PING {Account} <- {Address} {Status}", accountId, address, result.Status);
                    return;
                }

                Log.Information("PING {Account} <- {Address} {RTT} {Status}", accountId, result.Address, result.RoundtripTime, result.Status);
                IpToRTT.AddOrUpdate(result.Address, (ip) => result.RoundtripTime, (ip, rtt) => result.RoundtripTime);
                onSuccess?.Invoke(accountId, address, result.RoundtripTime);
            }
            catch (Exception ex)
            {
                Log.Error("PING {Account} with {address} was not success! {err}", accountId, address, ex);
            }
        }
    }

    public static void ClearPingedAccount(string accountId)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            return;
        }

        AccToPingedIPS.TryRemove(accountId, out _);
    }

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

        if (!AccToPingedIPS.TryGetValue(accountId, out var addresses))
        {
            return;
        }

        if (!addresses.Contains(address))
        {
            return;
        }

        List<IPAddress> withoutIP = addresses;
        withoutIP.Remove(address);
        AccToPingedIPS.TryUpdate(accountId, withoutIP, addresses);
    }
}

using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace DllNetwork;

public static class PingHelper
{
    private static readonly ConcurrentDictionary<string, List<IPAddress>> AccToPingedIPS = [];
    public static readonly ConcurrentDictionary<IPAddress, long> IpToRTT = [];

    public static async Task PingAddress(string AccountId, IPAddress address, Action<string, IPAddress, long>? onSuccess = null)
    {
        if (string.IsNullOrEmpty(AccountId))
            return;

        if (address == null)
            return;

        List<IPAddress> addresses = AccToPingedIPS.GetOrAdd(AccountId, []);
        if (!addresses.Contains(address))
        {
            AccToPingedIPS.TryUpdate(AccountId, [.. addresses, address], addresses);
            Log.Information("PING {Account} -> {ip}", AccountId, address);
            using Ping NetPing = new();
            try
            {
                PingReply result = await NetPing.SendPingAsync(address, 1000);
                if (result.Status != IPStatus.Success)
                {
                    Log.Information("PING {Account} <- {Address} {Status}", AccountId, address, result.Status);
                    return;
                }
                Log.Information("PING {Account} <- {Address} {RTT} {Status}", AccountId, result.Address, result.RoundtripTime, result.Status);
                IpToRTT.AddOrUpdate(result.Address, (ip) => result.RoundtripTime, (ip, rtt) => result.RoundtripTime);
                onSuccess?.Invoke(AccountId, address, result.RoundtripTime);
            }
            catch (Exception ex)
            {
                Log.Error("PING {Account} with {address} was not success! {err}", AccountId, address, ex);
            }
        }
    }

    public static void ClearPingedAccount(string AccountId)
    {
        if (string.IsNullOrEmpty(AccountId))
            return;

        AccToPingedIPS.TryRemove(AccountId, out _);
    }

    public static void ClearPingedAddress(string AccountId, IPAddress address)
    {
        if (string.IsNullOrEmpty(AccountId))
            return;

        if (address == null)
            return;

        if (!AccToPingedIPS.TryGetValue(AccountId, out var addresses))
            return;

        if (!addresses.Contains(address))
            return;

        List<IPAddress> withoutIP = addresses;
        withoutIP.Remove(address);
        AccToPingedIPS.TryUpdate(AccountId, withoutIP, addresses);
    }
}

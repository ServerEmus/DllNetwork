using DllNetwork.Json;
using DllNetwork.Managers;
using DllNetwork.Settings;
using Serilog;
using System.Text.Json;

namespace DllNetwork.Broadcast;

/// <summary>
/// Provides a custom broadcasting via http.
/// </summary>
public static class BroadcastCustom
{
    private static readonly HttpClient? Client;

    static BroadcastCustom()
    {
        string endPoint = NetworkSettings.Instance.Broadcast.CustomBroadcastServerEndpoint;
        if (string.IsNullOrEmpty(endPoint))
        {
            return;
        }

        Client = new()
        {
            BaseAddress = new Uri(endPoint),
        };
    }

    /// <summary>
    /// Sends a start message to the broadcast server.
    /// </summary>
    public static void Start()
    {
        if (Client == null)
        {
            return;
        }

        BroadcastAccount startJson = new()
        {
            AccountId = NetworkSettings.Instance.Account.AccountId,
            Addresses = [.. AddressHelper.Addresses.Select(static x => x.ToString())],
            Port = ServerManager.Instance.Port,
        };

        string data = JsonSerializer.Serialize(startJson, SourceGenerationContext.Default.BroadcastAccount);
        using var content = new StringContent(data);

        var response = Client.PostAsync("/start", content).Result;
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return;
        }

        try
        {
            string rsp = response.Content.ReadAsStringAsync().Result;
            NetworkLog.Logger.Warning("[BroadcastCustom.Start] Error: {code} {response}", response.StatusCode, rsp);
        }
        catch (Exception ex)
        {
            NetworkLog.Logger.Warning("[BroadcastCustom.Start] Error {ex}", ex);
        }
    }

    /// <summary>
    /// Sends a stop message to the broadcast server.
    /// </summary>
    public static void Stop()
    {
        if (Client == null)
        {
            return;
        }

        // We not really care about if account doesnt exists.
        Client.DeleteAsync($"/stop?accountId={NetworkSettings.Instance.Account.AccountId}");
    }

    /// <summary>
    /// Gets the broadcast accounts from the broadcast server.
    /// </summary>
    /// <returns>The broadcast accounts.</returns>
    public static List<BroadcastAccount> GetList()
    {
        List<BroadcastAccount> broadcasts = [];

        if (Client == null)
        {
            return broadcasts;
        }

        var httpResponse = Client.GetAsync("/list").Result;

        if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return broadcasts;
        }

        try
        {
            string rsp = httpResponse.Content.ReadAsStringAsync().Result;
            broadcasts = JsonSerializer.Deserialize(rsp, SourceGenerationContext.Default.ListBroadcastAccount) ?? [];
        }
        catch (Exception ex)
        {
            NetworkLog.Logger.Warning("[BroadcastCustom.GetList] Error {ex}", ex);
        }

        return broadcasts;
    }
}

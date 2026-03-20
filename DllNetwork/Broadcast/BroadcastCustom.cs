using DllNetwork.Json;
using DllNetwork.Managers;
using Serilog;
using System.Text.Json;

namespace DllNetwork.Broadcast;

public static class BroadcastCustom
{
    static readonly HttpClient? client;
    static BroadcastCustom()
    {
        string endPoint = NetworkSettings.Instance.Broadcast.CustomBroadcastServerEndpoint;
        if (string.IsNullOrEmpty(endPoint))
            return;

        client = new()
        {
            BaseAddress = new Uri(endPoint)
        };
    }

    public static void Start()
    {
        if (client == null)
            return;

        BroadcastJson startJson = new()
        { 
            AccountId = NetworkSettings.Instance.Account.AccountId,
            Addresses = [.. AddressHelper.Addresses.Select(static x => x.ToString())],
            Port = ServerManager.Instance.Port,
        };

        string data = JsonSerializer.Serialize(startJson, SourceGenerationContext.Default.BroadcastJson);

        var response = client.PostAsync("/start", new StringContent(data)).Result;
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return;

        try
        {
            string rsp = response.Content.ReadAsStringAsync().Result;
            Log.Warning("[BroadcastCustom.Start] Error: {code} {response}", response.StatusCode, rsp);
        }
        catch (Exception ex)
        {
            Log.Warning("[BroadcastCustom.Start] Error {ex}", ex);
        }

    }

    public static void Stop()
    {
        if (client == null)
            return;

        // We not really care about if account doesnt exists.
        client.DeleteAsync($"/stop?accountId={NetworkSettings.Instance.Account.AccountId}");
    }

    public static List<BroadcastJson> GetList()
    {
        List<BroadcastJson> broadcasts = [];

        if (client == null)
            return broadcasts;

        var httpResponse = client.GetAsync("/list").Result;

        if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            return broadcasts;

        try
        {
            string rsp = httpResponse.Content.ReadAsStringAsync().Result;
            broadcasts = JsonSerializer.Deserialize(rsp, SourceGenerationContext.Default.ListBroadcastJson) ?? [];
        }
        catch (Exception ex)
        {
            Log.Warning("[BroadcastCustom.GetList] Error {ex}", ex);
        }

        return broadcasts;
    }
}

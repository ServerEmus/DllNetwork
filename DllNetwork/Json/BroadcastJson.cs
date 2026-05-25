using System.Text.Json.Serialization;

namespace DllNetwork.Json;

public class BroadcastJson
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("addresses")]
    public List<string> Addresses { get; set; } = [];

    [JsonPropertyName("port")]
    public int Port { get; set; }

    public override string ToString()
    {
        return $"{AccountId} {Port} {string.Join(", ", Addresses)}";
    }
}

using System.Text.Json.Serialization;

namespace DllNetwork.Json;

/// <summary>
/// Represenet the details from the broadcasted account.
/// </summary>
public class BroadcastAccount
{
    /// <summary>
    /// Gets or sets the id of this account.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ip addresses.
    /// </summary>
    [JsonPropertyName("addresses")]
    public List<string> Addresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the port to connect the client to.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{AccountId} {Port} {string.Join(", ", Addresses)}";
    }
}

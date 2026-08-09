namespace DllNetwork.Settings;

/// <summary>
/// Represents the configuration settings for UDP broadcast communication.
/// </summary>
public class BroadcastSettings
{
    /// <summary>
    /// Gets or sets the network port used for UDP broadcast communication.
    /// </summary>
    public int BroadcastPort { get; set; } = 5555;

    /// <summary>
    /// Gets or sets the ending port number of the broadcast port range.
    /// </summary>
    public int EndRangeBroadcastPort { get; set; } = 5560;

    /// <summary>
    /// Gets or sets the endpoint for the custom broadcast server.
    /// </summary>
    /// <remarks>
    /// Endpoint communication is described in the BroadcastCommunication.md file.
    /// </remarks>
    public string CustomBroadcastServerEndpoint { get; set; } = string.Empty;
}
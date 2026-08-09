namespace DllNetwork.Settings;

/// <summary>
/// Represents the configuration settings for server network bindings, including the IPv4 and IPv6 addresses used to
/// accept incoming connections.
/// </summary>
public class BindingSettings
{
    /// <summary>
    /// Gets or sets the IPv4 address that the server binds to for incoming connections.
    /// </summary>
    public string BindIpv4 { get; set; } = "0.0.0.0";

    /// <summary>
    /// Gets or sets the IPv6 address that the server binds to for incoming connections.
    /// </summary>
    public string BindIpv6 { get; set; } = "::";
}

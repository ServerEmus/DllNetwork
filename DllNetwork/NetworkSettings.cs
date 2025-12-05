namespace DllNetwork;

/// <summary>
/// Represent a settings for the network module.
/// </summary>
public class NetworkSettings
{
    public static readonly NetworkSettings _instance = new();
    public static Func<NetworkSettings>? OnGet { get; set; }

    /// <summary>
    /// Gets the singleton instance of the NetworkSettings class.
    /// </summary>
    public static NetworkSettings Instance
    { 
        get
        {
            if (OnGet != null)
                return OnGet();
            return _instance;
        }
    }

    /// <summary>
    /// Gets or sets the settings that determine which content types are accepted in requests or responses.
    /// </summary>
    public AcceptSettings Accept { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration settings for the manager component.
    /// </summary>
    public ManagerSettings Manager { get; set; } = new();

    /// <summary>
    /// Gets or sets the broadcast configuration settings.
    /// </summary>
    public BroadcastSettings Broadcast { get; set; } = new();

    /// <summary>
    /// Gets or sets the binding configuration settings used for communication and data transfer.
    /// </summary>
    public BindingSettings Binding { get; set; } = new();
}

/// <summary>
/// Represents configuration options for accepting or rejecting incoming connections.
/// </summary>
public class AcceptSettings
{
    /// <summary>
    /// Accepts all connections.
    /// </summary>
    public bool AcceptAll { get; set; } = true;

    /// <summary>
    /// Accept only with this key.
    /// </summary>
    /// <remarks>
    /// This overrides <see cref="AcceptAll"/>.
    /// </remarks>
    public string AcceptKey { get; set; } = string.Empty;

    /// <summary>
    /// Reject message in hex string format.
    /// </summary>
    public string RejectHexString { get; set; } = string.Empty;
}

/// <summary>
/// Represents configuration settings for a manager, including channel count, statistics, and network options.
/// </summary>
public class ManagerSettings
{
    /// <summary>
    /// Gets or sets the number of channels supported.
    /// </summary>
    public byte ChannelCount { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether statistical analysis is enabled.
    /// </summary>
    public bool UseStatistics { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether IPv6 is enabled for network operations.
    /// </summary>
    public bool EnableIpv6 { get; set; }
}

/// <summary>
/// Represents the configuration settings for UDP broadcast communication.
/// </summary>
public class BroadcastSettings
{
    /// <summary>
    /// Gets or sets the network port used for UDP broadcast communication.
    /// </summary>
    public int BroadcastPort { get; set; } = 5555;
}

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
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
    /// Gets or sets the account configuration settings.
    /// </summary>
    public AccountConfig Account { get; set; } = new();

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

    /// <summary>
    /// Gets or sets the connection configuration settings.
    /// </summary>
    public ConnectionSettings Connection { get; set; } = new();
}

/// <summary>
/// Represents the configuration settings for network connections.
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// Gets or sets the handshake key used to establish a secure connection.
    /// </summary>
    public string HandshakeKey { get; set; } = string.Empty;
}

/// <summary>
/// Represents configuration settings for a manager.
/// </summary>
public class ManagerSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether IPv6 is enabled for network operations.
    /// </summary>
    public bool EnableIpv6 { get; set; }

    /// <summary>
    /// Gets or sets the maximum size of the network packet queue.
    /// </summary>
    public int MaxQueueSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the interval, in seconds, at which heartbeat messages are sent to maintain active connections.
    /// </summary>
    public byte HearthbeatInterval { get; set; } = 5;
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

    /// <summary>
    /// Gets or sets the ending port number of the broadcast port range.
    /// </summary>
    public int EndRangeBroadcastPort { get; set; } = 5560;
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

/// <summary>
/// Represents the configuration settings for a network account.
/// </summary>
public class AccountConfig
{
    public string AccountId { get; set; } = Guid.NewGuid().ToString();
    public List<string> DenyConnectionAccounts { get; set; } = [];
}

namespace DllNetwork.Settings;

/// <summary>
/// Represent a settings for the network module.
/// </summary>
public class NetworkSettings
{
    /// <summary>
    /// Default network settings values.
    /// </summary>
    public static readonly NetworkSettings Default = new();

    /// <summary>
    /// Gets or sets the custom function for getting the network settings values.
    /// </summary>
    public static Func<NetworkSettings>? OnGet { get; set; }

    /// <summary>
    /// Gets the singleton instance of the NetworkSettings class.
    /// </summary>
    public static NetworkSettings Instance
    {
        get
        {
            if (OnGet != null)
            {
                return OnGet();
            }

            return Default;
        }
    }

    /// <summary>
    /// Gets or sets the account configuration settings.
    /// </summary>
    public AccountSettings Account { get; set; } = new();

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

    /// <summary>
    /// Gets or sets the logging settings.
    /// </summary>
    public LogSettings Log { get; set; } = new();
}

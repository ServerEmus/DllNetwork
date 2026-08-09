namespace DllNetwork.Settings;

/// <summary>
/// Represents the configuration settings for a network account.
/// </summary>
public class AccountSettings
{
    /// <summary>
    /// Gets or sets the id of current account.
    /// </summary>
    public string AccountId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the list of denied account ids.
    /// </summary>
    public List<string> DenyConnectionAccounts { get; set; } = [];
}
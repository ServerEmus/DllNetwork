using Serilog.Events;

namespace DllNetwork.Settings;

/// <summary>
/// Represents configuration settings for logging.
/// </summary>
public class LogSettings
{
    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogEventLevel Level { get; set; } = LogEventLevel.Information;
}

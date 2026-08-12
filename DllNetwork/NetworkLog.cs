using DllNetwork.Settings;
using Serilog;

namespace DllNetwork;

/// <summary>
/// Provides a new logger for the networking.
/// </summary>
internal static class NetworkLog
{
    /// <summary>
    /// Gets the network-only logger.
    /// </summary>
    public static ILogger Logger
    {
        get
        {
            field ??= new LoggerConfiguration()
                    .MinimumLevel.Is(NetworkSettings.Instance.Log.Level)
                    .WriteTo.Logger(Log.Logger)
                    .CreateLogger();

            return field;
        }
    }
}

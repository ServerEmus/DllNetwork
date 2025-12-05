using DllNetwork;
using System.Text.Json;
using System.Text;

namespace NetworkTest.Json;

internal static class NetworkSettingsJson
{
    static readonly string FileName = "network_settings.json";

    public static void Connect()
    {
        NetworkSettings.OnGet += NetworkSettings_OnGet;
    }

    public static void Disconnect()
    {
        NetworkSettings.OnGet -= NetworkSettings_OnGet;
    }

    private static NetworkSettings NetworkSettings_OnGet()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "configs" ,FileName);
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(NetworkSettings._instance, SourceGenerationContext.Default.NetworkSettings));
            return NetworkSettings._instance;
        }
        string json = File.ReadAllText(path, Encoding.UTF8);
        NetworkSettings? settings = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.NetworkSettings);
        if (settings == null)
            return NetworkSettings._instance;
        return settings;
    }
}

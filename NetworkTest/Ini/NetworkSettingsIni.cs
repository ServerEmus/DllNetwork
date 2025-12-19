using DllNetwork;
using IniParser;
using IniParser.Model;

namespace NetworkTest.Ini;

internal static class NetworkSettingsIni
{
    static readonly FileIniDataParser parser = new();
    static bool AlreadyRead;
    static readonly string FileName = "network_settings.ini";

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
        if (AlreadyRead)
            return NetworkSettings._instance;

        string path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            IniData parsedData = IniSourceGeneration.WriteNetworkSettingsIniData(NetworkSettings._instance);
            parser.WriteFile(path, parsedData, System.Text.Encoding.UTF8);
            AlreadyRead = true;
            return NetworkSettings._instance;
        }
        IniData data = parser.ReadFile(path, System.Text.Encoding.UTF8);
        IniSourceGeneration.ReadNetworkSettingsIniData(NetworkSettings._instance, data);
        AlreadyRead = true;
        return NetworkSettings._instance;
    }
}

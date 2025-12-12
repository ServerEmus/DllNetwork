using DllNetwork.Broadcast;
using DllNetwork.Main;
using NetworkTest.Ini;

namespace NetworkTest;

internal class Program
{
    static void Main(string[] _)
    {
        Console.WriteLine("Hello, World!");
        Shared.MainLogger.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileName = $"networktest.txt";
        Shared.MainLogger.CreateNew();
        NetworkSettingsIni.Connect();
        BroadcastManager.Instance.Start();
        MainManager.Instance.Start();
        bool Stop = false;
        Thread thread = new(() =>
        {
            while (!Stop)
            {
                BroadcastManager.Instance.Update();
                MainManager.Instance.Update();
            }
        });
        thread.Start();
        string? readed = null;
        while (readed?.ToLower() != "q")
        {
            readed = Console.ReadLine();
            if (readed == null)
                continue;
            if (readed.Contains("test"))
            {
                MainManager.Instance.SendBroadcast();
            }
        }
        Stop = true;
        thread.Join(10);
        MainManager.Instance.Stop();
        BroadcastManager.Instance.Stop();
    }
}

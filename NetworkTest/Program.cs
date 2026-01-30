using DllNetwork;
using DllNetwork.Formatters;
using NetworkTest.Ini;
using Serilog;
using System.Net;

namespace NetworkTest;

internal class Program
{
    static void Main(string[] _)
    {
        Console.WriteLine("Your IP addresses:");
        foreach (var item in AddressHelper.GetInteraceAddresses())
        {
            Console.WriteLine(item);
        }

        Shared.MainLogger.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileName = $"networktest.txt";
        Shared.MainLogger.CreateNew();
        Formatters.RegisterAll();
        NetworkSettingsIni.Connect();
        MainNetwork.Instance.Start();

        string? readed = null;
        while (readed?.ToLower() != "q")
        {
            readed = Console.ReadLine();
            MainNetwork.Instance.Update();
            if (readed == null)
                continue;
            if (readed.Contains("bc"))
            {
                MainNetwork.Instance.BroadcastWork.SendAnnounce();
            }
            if (readed.StartsWith('!') && readed.Contains(' '))
            {

            }
        }
        MainNetwork.Instance.Stop();
    }


}

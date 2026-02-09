using DllNetwork;
using DllNetwork.Formatters;
using DllNetwork.Packets;
using NetworkTest.Ini;
using Serilog;
using System.Net;

namespace NetworkTest;

internal class Program
{
    static void Main(string[] _)
    {
        Console.WriteLine("Your IP addresses:");
        foreach (var item in AddressHelper.GetInterfaceAddresses())
        {
            Console.WriteLine(item);
        }

        File.Delete("networktest.txt");
        Shared.MainLogger.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileName = $"networktest.txt";
        Shared.MainLogger.CreateNew();
        Formatters.RegisterAll();
        NetworkSettingsIni.Connect();
        MainNetwork.Instance.Start();

        string? input = null;
        while (input?.ToLower() != "q")
        {
            input = Console.ReadLine();
            MainNetwork.Instance.Update();
            if (input == null)
                continue;
            if (input.Contains("bc"))
            {
                MainNetwork.Instance.BroadcastWork.SendAnnounce();
            }
            if (input.StartsWith('!') && input.Contains(' '))
            {
                Console.WriteLine("Here should be message sending!");
            }
        }
        MainNetwork.Instance.Stop();
    }


}

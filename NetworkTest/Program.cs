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
        /*
        AnnouncePacket announcePacket = new();
        var x = PackExt.Serialize(announcePacket);
        announcePacket = x.Deserialize<AnnouncePacket>()!;

        ConnectPacket connectPacket  = new();
        x = PackExt.Serialize(connectPacket);
        connectPacket = x.Deserialize<ConnectPacket>()!;

        ConnectReplyPacket connectReply = new();
        x = PackExt.Serialize(connectReply);
        connectReply = x.Deserialize<ConnectReplyPacket>()!;

        HeartBeatPacket heartBeatPacket = new();
        x = PackExt.Serialize(heartBeatPacket);
        heartBeatPacket = x.Deserialize<HeartBeatPacket>()!;

        announcePacket = new();
        x = PackExt.Serialize(announcePacket);
        announcePacket = (AnnouncePacket)x.Deserialize<INetworkPacket>()!;

        connectPacket = new();
        x = PackExt.Serialize(connectPacket);
        connectPacket = (ConnectPacket)x.Deserialize<INetworkPacket>()!;

        connectReply = new();
        x = PackExt.Serialize(connectReply);
        connectReply = (ConnectReplyPacket)x.Deserialize<INetworkPacket>()!;

        heartBeatPacket = new();
        x = PackExt.Serialize(heartBeatPacket);
        heartBeatPacket = (HeartBeatPacket)x.Deserialize<INetworkPacket>()!;
        */

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

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
        foreach (var item in AddressHelper.GetInteraceAddresses())
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
                Console.WriteLine("Here should be message sending!");
            }
        }
        MainNetwork.Instance.Stop();
    }


}

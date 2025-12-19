using DllNetwork;
using DllNetwork.Broadcast;
using DllNetwork.Main;
using LiteNetLib;
using NetworkTest.CustomPacket;
using NetworkTest.Ini;
using Serilog;

namespace NetworkTest;

internal class Program
{
    static void Main(string[] _)
    {
        Console.WriteLine("Hello, World!");

        foreach (var item in AddressHelper.GetInteraceAddresses())
        {
            Console.WriteLine(item);
        }
        


        Shared.MainLogger.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileName = $"networktest.txt";
        Shared.MainLogger.CreateNew();
        PacketProcessor.Processor.RegisterNestedType<MessagePacket>();
        PacketProcessor.Processor.SubscribeNetSerializable<MessagePacket, ReceiveUserData>(MessageWorker);
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
        MessagePacket message = new();
        thread.Start();
        string? readed = null;
        while (readed?.ToLower() != "q")
        {
            readed = Console.ReadLine();
            if (readed == null)
                continue;
            if (readed.Contains("bc"))
            {
                MainManager.Instance.SendBroadcast();
            }
            if (readed.StartsWith('!') && readed.Contains(' '))
            {
                Log.Debug("send msg!");
                try
                {
                    var splitted = readed.Split(' ');
                    var acid = splitted[0].Replace("!", string.Empty);
                    var msg = splitted[1..];
                    message.Message = string.Join(" ", msg);
                    Log.Debug("Account: {0}", acid);
                    Log.Debug("sending! {0}", message.Message);
                    MainManager.SendToAccount(acid, ref message);
                    Log.Debug("sent!");
                }
                catch (Exception ex)
                {
                    Log.Error("Error! {er}", ex);
                }

            }
        }
        Stop = true;
        thread.Join(10);
        MainManager.Instance.Stop();
        BroadcastManager.Instance.Stop();
    }

    private static void MessageWorker(MessagePacket packet, ReceiveUserData data)
    {
        Log.Information("Message got: {msg} from {data}", packet.Message, data);
    }
}

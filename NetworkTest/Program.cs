using DllNetwork;
using DllNetwork.Broadcast;
using DllNetwork.Managers;
using NetworkTest.CustomPacket;
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
            Log.Information("Address: {address}", item);
        }

        File.Delete("networktest.txt");
        Shared.MainLogger.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        Shared.MainLogger.FileName = $"networktest.txt";
        Shared.MainLogger.CreateNew();
        PacketProcessor.Processor.SubscribeNetSerializable<MessagePacket, ReceiveData>(MessagePacket.OnReceived);
        NetworkSettingsIni.Connect();

        ServerManager.Instance.Start();
        ClientManager.Instance.Start();
        Log.Information("Id: {id}", NetworkSettings.Instance.Account.AccountId);
        Log.Information("Server started on port: {port}", ServerManager.Instance.Port);
        ClientManager.Instance.Connect(new IPEndPoint(IPAddress.Loopback, ServerManager.Instance.Port), NetworkSettings.Instance.Account.AccountId);

        string? input = null;
        while (input?.ToLower() != "q")
        {
            input = Console.ReadLine();

            ServerManager.Instance.Update();
            ClientManager.Instance.Update();
            BroadcastUdp.UdpUpdate();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.Contains("bc start"))
            {
                BroadcastUdp.Start();
                BroadcastCustom.Start();
            }

            if (input.Contains("bc stop"))
            {
                BroadcastUdp.Stop();
                BroadcastCustom.Stop();
            }

            if (input.Contains("bc list"))
            {
                Log.Information("{ids}", BroadcastUdp.GetList());
                Log.Information("{ids}", BroadcastCustom.GetList());
            }

            if (input.Contains("connect"))
            {
                foreach (var item in BroadcastUdp.GetList())
                {
                    if (item.Addresses.Count == 0)
                        continue;

                    string? addressToConnect = item.Addresses.FirstOrDefault();

                    if (string.IsNullOrEmpty(addressToConnect))
                        continue;

                    if (ClientManager.Instance.IsAccountConnected(item.AccountId))
                        continue;

                    ClientManager.Instance.Connect(addressToConnect, item.Port, item.AccountId);
                }

                foreach (var item in BroadcastCustom.GetList())
                {
                    if (item.Addresses.Count == 0)
                        continue;

                    string? addressToConnect = item.Addresses.FirstOrDefault();

                    if (string.IsNullOrEmpty(addressToConnect))
                        continue;

                    if (ClientManager.Instance.IsAccountConnected(item.AccountId))
                        continue;

                    ClientManager.Instance.Connect(addressToConnect, item.Port, item.AccountId);
                }
            }

            if (input.Contains("ping info"))
            {
                foreach (var item in PingHelper.IpToRTT)
                {
                    Log.Information("Ping info: {ip} {rtt}", item.Key, item.Value);
                }
            }

            if (input.StartsWith("c!") && input.Contains(' '))
            {
                string[] data = input[1..].Split(' ', 2);
                string account = data[0];
                string msg = data[1];
                ClientManager.Instance.Send(new MessagePacket() { Message = msg }, account);
            }

            if (input.StartsWith("s!") && input.Contains(' '))
            {
                string[] data = input[1..].Split(' ', 2);
                string account = data[0];
                string msg = data[1];
                ServerManager.Instance.Send(new MessagePacket() { Message = msg }, account);
            }
        }

        ClientManager.Instance.Stop();
        ServerManager.Instance.Stop();

    }
}

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DllNetwork;

public static class AddressHelper
{
    private static bool WhereCheck(NetworkInterface networkInterface)
    {
        if (networkInterface.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
            return false;
        if (networkInterface.OperationalStatus is not OperationalStatus.Up)
            return false;
        return networkInterface.GetIPProperties().UnicastAddresses.Count > 0;
    }

    private static void GetIpAddress(IPInterfaceProperties properties, ref List<string> ips)
    {
        foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
        {
            IPAddress address = unicastAddress.Address;
            if (address.IsIPv6LinkLocal || address.IsIPv6Teredo)
                continue;
            if (address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
            {
                ips.Add(address.ToString());
            }
        }
    }

    public static List<string> GetInteraceAddresses()
    {
        List<string> addresses = [];
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(WhereCheck);
            foreach (var ipv4 in interfaces)
            {
                GetIpAddress(ipv4.GetIPProperties(), ref addresses);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        if (addresses.Count == 0)
            addresses.Add("127.0.0.1");

        return addresses;
    }
}

using DllNetwork.Settings;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DllNetwork;

/// <summary>
/// Provides a helper class for ip addresses.
/// </summary>
public static class AddressHelper
{
    /// <summary>
    /// Gets the usable addresses list.
    /// </summary>
    public static List<IPAddress> Addresses
    {
        get
        {
            if (field == null)
            {
                field = GetInterfaceAddresses();

                if (!NetworkSettings.Instance.Manager.EnableIpv6)
                {
                    field.RemoveAll(static x => x.AddressFamily == AddressFamily.InterNetworkV6);
                }
            }

            return field;
        }
    }

    /// <summary>
    /// Gets the ip addresses.
    /// </summary>
    /// <returns>The usable ip addresses.</returns>
    /// <remarks>
    /// Filters out the loopback, turned off interface.
    /// </remarks>
    public static List<IPAddress> GetInterfaceAddresses()
    {
        List<IPAddress> addresses = [];

        var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(WhereCheck);
        foreach (var @interface in interfaces)
        {
            GetIpAddress(@interface.GetIPProperties(), ref addresses);
        }

        if (addresses.Count == 0)
        {
            addresses.Add(IPAddress.Loopback);
        }

        return addresses;
    }

    /// <summary>
    /// Checks whenever the <paramref name="port"/> is in use.
    /// </summary>
    /// <param name="port">The port to check.</param>
    /// <param name="isTcp">Whenever the port is used by tcp or udp.</param>
    /// <returns><see langword="true"/> if port is being used; otherwise, <see langword="false"/>.</returns>
    public static bool IsPortInUse(int port, bool isTcp = true)
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] listeners = isTcp ? properties.GetActiveTcpListeners() : properties.GetActiveUdpListeners();

        return listeners.Any(x => x.Port == port);
    }

    /// <summary>
    /// Gets a valid port in range.
    /// </summary>
    /// <param name="startPort">The port to start the check.</param>
    /// <param name="endPort">The exclusive end port to check.</param>
    /// <param name="isTcp">Whenever the port to be tcp to udp.</param>
    /// <returns>The valid port that can be used; otherwise, 0.</returns>
    public static int GetPort(int startPort = 7777, int endPort = 8000, bool isTcp = true)
    {
        for (int port = startPort; port < endPort; port++)
        {
            if (!IsPortInUse(port, isTcp))
            {
                return port;
            }
        }

        return 0;
    }

    private static bool WhereCheck(NetworkInterface networkInterface)
    {
        if (networkInterface.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        {
            return false;
        }

        if (networkInterface.OperationalStatus is not OperationalStatus.Up)
        {
            return false;
        }

        return networkInterface.GetIPProperties().UnicastAddresses.Count > 0;
    }

    private static void GetIpAddress(IPInterfaceProperties properties, ref List<IPAddress> ips)
    {
        foreach (IPAddress address in properties.UnicastAddresses.Select(x => x.Address)
            .Where(x => !x.IsIPv6LinkLocal && !x.IsIPv6Teredo && (x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6)))
        {
            ips.Add(address);
        }
    }
}

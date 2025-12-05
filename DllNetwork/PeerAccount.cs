using DllNetwork.Broadcast;
using DllNetwork.Main;
using LiteNetLib;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DllNetwork;

/// <summary>
/// Represent a network account with multiple endpoint support.
/// </summary>
public class PeerAccount
{
    /// <summary>
    /// Storage for the accountId and the peer.
    /// </summary>
    public readonly static Dictionary<string, PeerAccount> AccountToPeer = [];

    /// <summary>
    /// Adds the specified endpoint to the collection associated with the given account identifier if it does not
    /// already exist.
    /// </summary>
    /// <typeparam name="TPoint">The type of endpoint to add. Must implement the IPEndPoint interface.</typeparam>
    /// <param name="accountId">The unique identifier of the account to which the endpoint will be added. Cannot be null.</param>
    /// <param name="endPoint">The endpoint to associate with the specified account. Cannot be null.</param>
    public static void TryAdd<TPoint>(string accountId, TPoint endPoint) where TPoint : IPEndPoint
    {
        if (!AccountToPeer.TryGetValue(accountId, out var account))
        {
            AccountToPeer[accountId] = account = new()
            { 
                AccountId = accountId,
            };
        }

        account.EndPoints.Add(endPoint);

        Log.Debug("Account {Id} endpoints now: \n{endpoints}", accountId, string.Join(", ", account.EndPoints));
    }

    /// <summary>
    /// Removes the association for the specified account identifier from the internal mapping.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account whose association is to be removed. Cannot be null.</param>
    public static void Remove(string accountId)
    {
        AccountToPeer.Remove(accountId);
    }
    
    public static bool TryGetAccountId<TPoint>(TPoint point, [NotNullWhen(true)] out string accountId) where TPoint : IPEndPoint
    {
        accountId = string.Empty;

        if (TryGetAccount(point, out PeerAccount? account))
        {
            accountId = account.AccountId;
            return true;
        }

        return false;
    }

    public static bool TryGetAccount<TPoint>(TPoint point, [NotNullWhen(true)] out PeerAccount? account) where TPoint : IPEndPoint
    {
        account = null;

        foreach (var peerAccount in AccountToPeer.Values)
        {
            if (peerAccount.EndPoints.Contains(point))
            {
                account = peerAccount;
                return true;
            }
        }

        return false;
    }

    public static bool TryGetAccount(string accountId, [NotNullWhen(true)] out PeerAccount? account)
    {
        return AccountToPeer.TryGetValue(accountId, out account);
    }

    public static IEnumerable<NetPeer> GetAllBroadcastPeers
    {
        get
        {
            return AccountToPeer.Values.SelectMany(peer => peer.BroadcastPeers);
        }
    }

    public static IEnumerable<NetPeer> GetAllMainPeers
    {
        get
        {
            return AccountToPeer.Values.SelectMany(peer => peer.MainPeers);
        }
    }


    public static IEnumerable<NetPeer> GetAllPeers
    {
        get
        {
            return AccountToPeer.Values.SelectMany(peer => peer.Peers);
        }
    }

    /// <summary>
    /// An account identifier.
    /// </summary>
    public string AccountId = string.Empty;

    /// <summary>
    /// List of endpoints the user has sent.
    /// </summary>
    public List<IPEndPoint> EndPoints = [];


    public IEnumerable<NetPeer> Peers
    {
        get
        {
            return EndPoints.Where(static x => x is NetPeer peer).Select(static x => (NetPeer)x);
        }
    }

    public IEnumerable<NetPeer> BroadcastPeers
    {
        get
        {
            return Peers.Where(static x => x.NetManager is BroadcastManager);
        }
    }

    public IEnumerable<NetPeer> MainPeers
    {
        get
        {
            return Peers.Where(static x => x.NetManager is MainManager);
        }
    }
}

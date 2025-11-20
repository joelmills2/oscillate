using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    // Server-side list of all players
    public static readonly List<NetworkPlayer> ServerPlayers = new List<NetworkPlayer>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (!ServerPlayers.Contains(this))
                ServerPlayers.Add(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            ServerPlayers.Remove(this);
        }
    }
}

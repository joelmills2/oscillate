/*
https://www.youtube.com/watch?v=kVt0I6zZsf0
*/
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
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

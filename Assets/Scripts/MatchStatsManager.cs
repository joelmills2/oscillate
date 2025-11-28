using UnityEngine;
using Unity.Netcode;

public class MatchStatsManager : NetworkBehaviour
{
    public static MatchStatsManager Instance;

    public NetworkVariable<int> EnemyDeathCount = new NetworkVariable<int>();

    void Awake()
    {
        Instance = this;
    }
}
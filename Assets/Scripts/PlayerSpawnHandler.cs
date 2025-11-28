using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        var manager = SpawnPointManager.Instance;
        if (manager == null) return;

        Transform spawn = manager.GetSpawnPoint(OwnerClientId);
        if (spawn == null) return;

        transform.SetPositionAndRotation(spawn.position, spawn.rotation);
    }
}
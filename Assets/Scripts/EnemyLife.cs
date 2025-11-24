using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;

public class EnemyLife : NetworkBehaviour
{
    [SerializeField] NetworkObject nextVariantPrefab;
    [SerializeField] float replaceDelay = 1f;

    Health health;
    NavMeshAgent agent;

    public override void OnNetworkSpawn()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();

        if (IsServer && health != null)
        {
            StartCoroutine(ServerLifeLoop());
        }
    }

    IEnumerator ServerLifeLoop()
    {
        while (true)
        {
            if (health.IsDead)
            {
                HandleDeath();
                yield return new WaitForSeconds(replaceDelay);
                HandleReplace();
                yield break;
            }
            yield return null;
        }
    }

    void HandleDeath()
    {
        if (agent != null) agent.isStopped = true;
    }

    void HandleReplace()
    {
        if (nextVariantPrefab != null)
        {
            NetworkObject obj = Instantiate(nextVariantPrefab, transform.position, transform.rotation);
            obj.Spawn();
        }

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

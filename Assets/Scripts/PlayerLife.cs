using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerLife : NetworkBehaviour
{
    [SerializeField] float respawnDelay = 3f;
    [SerializeField] float invincibilityTime = 2f;

    Health health;
    PlayerMovement movement;
    CapsuleCollider capsule;
    Rigidbody rb;

    public float RespawnDelay => respawnDelay;

    public override void OnNetworkSpawn()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        capsule = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if (IsServer && health != null)
        {
            StartCoroutine(ServerLifeLoop());
        }
    }

    IEnumerator ServerLifeLoop()
    {
        while (true)
        {
            if (health != null && health.IsDead)
            {
                HandleDeathServer();
                yield return new WaitForSeconds(respawnDelay);
                HandleRespawnServer();
            }
            yield return null;
        }
    }

    void HandleDeathServer()
    {
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }

    void HandleRespawnServer()
    {
        Transform spawn = null;
        if (SpawnPointManager.Instance != null)
        {
            spawn = SpawnPointManager.Instance.GetSpawnPoint(OwnerClientId);
        }

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        if (spawn != null)
        {
            pos = spawn.position;
            rot = spawn.rotation;
        }

        RespawnClientRpc(pos, rot);

        if (health != null)
        {
            health.ResetHealth();
            health.SetInvincible(true);
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;

        if (rb != null) rb.linearVelocity = Vector3.zero;
        if (capsule != null) capsule.enabled = true;
        if (movement != null) movement.enabled = true;
    }

    IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibilityTime);
        if (health != null) health.SetInvincible(false);
    }
}

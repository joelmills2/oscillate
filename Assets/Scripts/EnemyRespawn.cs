using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyRespawn : NetworkBehaviour
{
    [SerializeField] Health health;
    [SerializeField] float respawnDelay = 3f;

    [SerializeField] int healthIncreasePerLevel = 20;
    [SerializeField] float chaseSpeedIncreasePerLevel = 0.5f;
    [SerializeField] float fireCooldownDecreasePerLevel = 0.05f;

    NavMeshAgent agent;
    EnemyAI enemyAI;
    Vector3 initialPos;
    Quaternion initialRot;
    int baseMaxHealth;
    float baseChaseSpeed;
    float baseFireCooldown;
    int levelIndex;

    readonly List<GameObject> childObjects = new List<GameObject>();
    readonly List<bool> childOriginalStates = new List<bool>();

    public override void OnNetworkSpawn()
    {
        if (!health) health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
        initialPos = transform.position;
        initialRot = transform.rotation;

        if (health != null)
            baseMaxHealth = health.MaxHealth;

        if (enemyAI != null)
        {
            baseChaseSpeed = enemyAI.ChaseSpeed;
            baseFireCooldown = enemyAI.FireCooldown;
        }

        CacheChildren();

        if (IsServer && health != null)
            StartCoroutine(LifeLoop());
    }

    void CacheChildren()
    {
        childObjects.Clear();
        childOriginalStates.Clear();

        Transform[] all = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == transform) continue;
            GameObject go = all[i].gameObject;
            childObjects.Add(go);
            childOriginalStates.Add(go.activeSelf);
        }
    }

    IEnumerator LifeLoop()
    {
        bool wasDead = false;

        while (true)
        {
            bool dead = health.IsDead;

            if (dead && !wasDead)
            {
                HandleDeath();
                yield return new WaitForSeconds(respawnDelay);
                HandleRespawnAndLevelUp();
            }

            wasDead = dead;
            yield return null;
        }
    }

    void HandleDeath()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        SetVisibleClientRpc(false);
    }

    void HandleRespawnAndLevelUp()
    {
        levelIndex++;

        int newMaxHealth = baseMaxHealth + healthIncreasePerLevel * levelIndex;
        if (health != null)
        {
            health.SetMaxHealth(newMaxHealth);
            health.ResetHealth();
        }

        if (enemyAI != null)
        {
            float newChaseSpeed = baseChaseSpeed + chaseSpeedIncreasePerLevel * levelIndex;
            float newFireCooldown = baseFireCooldown - fireCooldownDecreasePerLevel * levelIndex;
            enemyAI.ChaseSpeed = newChaseSpeed;
            enemyAI.FireCooldown = newFireCooldown;
        }

        transform.position = initialPos;
        transform.rotation = initialRot;

        SetVisibleClientRpc(true);

        if (agent != null)
            agent.isStopped = false;
    }

    [ClientRpc]
    void SetVisibleClientRpc(bool visible)
    {
        for (int i = 0; i < childObjects.Count; i++)
        {
            GameObject go = childObjects[i];
            if (go == null) continue;

            bool targetState = visible ? childOriginalStates[i] : false;
            go.SetActive(targetState);
        }
    }
}

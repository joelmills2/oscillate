using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [SerializeField] int maxHealth = 50;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    bool invincible;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            invincible = false;
            SyncHealthClientRpc(CurrentHealth.Value, maxHealth);
        }
    }

    public void Damage(int amount)
    {
        if (!IsServer) return;
        if (invincible) return;
        if (amount <= 0) return;
        if (CurrentHealth.Value <= 0) return;

        int next = Mathf.Max(CurrentHealth.Value - amount, 0);
        CurrentHealth.Value = next;
        SyncHealthClientRpc(CurrentHealth.Value, maxHealth);
    }

    public void ResetHealth()
    {
        if (!IsServer) return;
        CurrentHealth.Value = maxHealth;
        SyncHealthClientRpc(CurrentHealth.Value, maxHealth);
    }

    public void SetInvincible(bool value)
    {
        if (!IsServer) return;
        invincible = value;
    }

    public void SetMaxHealth(int value)
    {
        if (!IsServer) return;
        maxHealth = Mathf.Max(1, value);
        if (CurrentHealth.Value > maxHealth)
            CurrentHealth.Value = maxHealth;
        SyncHealthClientRpc(CurrentHealth.Value, maxHealth);
    }

    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth.Value <= 0;

    public float Normalized => maxHealth > 0 ? CurrentHealth.Value / (float)maxHealth : 0f;

    [ClientRpc]
    void SyncHealthClientRpc(int current, int max)
    {
        var bars = GetComponentsInChildren<EnemyHealthBar>(true);
        for (int i = 0; i < bars.Length; i++)
            bars[i].SetFromValues(current, max);
    }
}
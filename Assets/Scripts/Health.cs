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
    }

    public void ResetHealth()
    {
        if (!IsServer) return;
        CurrentHealth.Value = maxHealth;
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
    }

    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth.Value <= 0;
}

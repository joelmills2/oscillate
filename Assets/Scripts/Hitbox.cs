using UnityEngine;

public enum HitboxTeam
{
    Player,
    Enemy
}

public class Hitbox : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] float damageMultiplier = 1f;
    [SerializeField] HitboxTeam team;

    public HitboxTeam Team => team;

    void Awake()
    {
        if (!health) health = GetComponentInParent<Health>();
    }

    public void ApplyDamage(int baseDamage)
    {
        if (!health) return;
        int amount = Mathf.RoundToInt(baseDamage * damageMultiplier);
        health.Damage(amount);
    }
}

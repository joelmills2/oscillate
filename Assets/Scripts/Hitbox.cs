using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] float damageMultiplier = 1f;

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

using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] float lifeSeconds = 5f;
    [SerializeField] int damage = 1;

    Collider selfCol;
    float lifeTimer;

    void Awake()
    {
        selfCol = GetComponent<Collider>();
    }

    public void Init(Collider[] ignoreThese)
    {
        if (selfCol == null)
            selfCol = GetComponent<Collider>();

        for (int i = 0; i < ignoreThese.Length; i++)
        {
            if (ignoreThese[i] && selfCol)
            {
                Physics.IgnoreCollision(selfCol, ignoreThese[i], true);
            }
        }
    }

    void Update()
    {
        if (!IsServer) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeSeconds)
        {
            DespawnProjectile();
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (!IsServer) return;

        Hitbox hitbox = c.collider.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.ApplyDamage(damage);
        }

        DespawnProjectile();
    }

    void DespawnProjectile()
    {
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

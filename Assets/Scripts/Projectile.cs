using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] float lifeSeconds = 5f;
    [SerializeField] int damage = 10;
    [SerializeField] HitboxTeam targetTeam;
    [SerializeField] float initialSpeed = 30f;

    Collider selfCol;
    float lifeTimer;
    ulong shooterClientId;
    WeaponType weaponType = WeaponType.None;

    void Awake()
    {
        selfCol = GetComponent<Collider>();
    }

    public void Init(Collider[] ignoreThese, ulong shooterId, Vector3 dir, WeaponType wType)
    {
        if (selfCol == null)
            selfCol = GetComponent<Collider>();

        for (int i = 0; i < ignoreThese.Length; i++)
        {
            if (ignoreThese[i] && selfCol)
                Physics.IgnoreCollision(selfCol, ignoreThese[i], true);
        }

        shooterClientId = shooterId;
        weaponType = wType;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = dir * initialSpeed;

        InitClientRpc(shooterId, dir);
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    public void SetTargetTeam(HitboxTeam team)
    {
        targetTeam = team;
    }

    [ClientRpc]
    void InitClientRpc(ulong shooterId, Vector3 dir)
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(shooterId, out var client)) return;

        GameObject shooterObj = client.PlayerObject != null ? client.PlayerObject.gameObject : null;
        if (shooterObj == null) return;

        Collider[] shooterCols = shooterObj.GetComponentsInChildren<Collider>();
        if (selfCol == null) selfCol = GetComponent<Collider>();

        for (int i = 0; i < shooterCols.Length; i++)
        {
            if (shooterCols[i] && selfCol)
                Physics.IgnoreCollision(selfCol, shooterCols[i], true);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = dir * initialSpeed;
    }

    void Update()
    {
        if (!IsServer) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeSeconds)
            DespawnProjectile();
    }

    void OnCollisionEnter(Collision c)
    {
        if (!IsServer) return;

        bool didDamage = false;

        Hitbox hitbox = c.collider.GetComponent<Hitbox>();
        if (hitbox != null && hitbox.Team == targetTeam)
        {
            hitbox.ApplyDamage(damage);
            didDamage = true;

            if (WeaponLevelManager.Instance != null && weaponType == WeaponType.Bow)
                WeaponLevelManager.Instance.AddDamage(WeaponType.Bow, damage);
        }

        if (didDamage)
            HitConfirmClientRpc(shooterClientId);

        DespawnProjectile();
    }

    [ClientRpc]
    void HitConfirmClientRpc(ulong shooterClientIdArg)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.LocalClientId != shooterClientIdArg) return;
        if (Hitmarker.LocalInstance != null)
            Hitmarker.LocalInstance.ShowHitmarker();
    }

    void DespawnProjectile()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn(true);
        else
            Destroy(gameObject);
    }
}

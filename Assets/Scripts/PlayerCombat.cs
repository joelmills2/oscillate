using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public enum WeaponType
{
    None,
    Sword,
    Bow
}

public enum WeaponRarity
{
    Bronze = 1,
    Silver = 2,
    Gold = 3
}

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] InputActionReference rangedAttackAction;
    [SerializeField] InputActionReference meleeAttackAction;
    [SerializeField] InputActionReference pickupAction;
    [SerializeField] InputActionReference dropAction;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform bowArrowSpawnPoint;
    [SerializeField] NetworkObject arrowProjectilePrefab;

    [SerializeField] float rangedCooldown = 0.4f;
    [SerializeField] int bowBaseDamage = 10;

    [SerializeField] float meleeCooldown = 0.4f;
    [SerializeField] float meleeRange = 2f;
    [SerializeField] float meleeRadius = 1f;
    [SerializeField] LayerMask meleeHitMask;
    [SerializeField] int swordBaseDamage = 15;

    [SerializeField] float pickupRadius = 2f;
    [SerializeField] float dropDistance = 1.5f;
    [SerializeField] NetworkObject swordPickupPrefab;
    [SerializeField] NetworkObject bowPickupPrefab;

    [SerializeField] PlayerWeaponView weaponView;
    [SerializeField] Health health;

    WeaponType equippedWeaponType = WeaponType.None;
    WeaponRarity equippedRarity = WeaponRarity.Bronze;
    float nextRangedTime;
    float nextMeleeTime;

    public WeaponType CurrentWeaponType => equippedWeaponType;
    public WeaponRarity CurrentWeaponRarity => equippedRarity;

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (rangedAttackAction) rangedAttackAction.action.Enable();
        if (meleeAttackAction) meleeAttackAction.action.Enable();
        if (pickupAction) pickupAction.action.Enable();
        if (dropAction) dropAction.action.Enable();
    }

    void OnDisable()
    {
        if (rangedAttackAction) rangedAttackAction.action.Disable();
        if (meleeAttackAction) meleeAttackAction.action.Disable();
        if (pickupAction) pickupAction.action.Disable();
        if (dropAction) dropAction.action.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        WeaponType startType = WeaponType.Bow;
        if (NetworkManager.Singleton != null &&
            OwnerClientId == NetworkManager.ServerClientId)
        {
            startType = WeaponType.Sword;
        }
        WeaponRarity rarity = WeaponRarity.Bronze;
        if (WeaponLevelManager.Instance != null && startType != WeaponType.None)
            rarity = WeaponLevelManager.Instance.GetRarity(startType);

        EquipWeapon(startType, rarity);
    }


    void Update()
    {
        if (!IsOwner) return;
        if (health != null && health.IsDead) return;

        if (rangedAttackAction && rangedAttackAction.action.WasPressedThisFrame())
            TryRangedAttack();

        if (meleeAttackAction && meleeAttackAction.action.WasPressedThisFrame())
            TryMeleeAttack();

        if (pickupAction && pickupAction.action.WasPressedThisFrame())
            TryPickup();

        if (dropAction && dropAction.action.WasPressedThisFrame())
            TryDrop();
    }

    void TryRangedAttack()
    {
        if (equippedWeaponType != WeaponType.Bow) return;
        if (Time.time < nextRangedTime) return;
        if (!playerCamera) return;

        nextRangedTime = Time.time + rangedCooldown;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * 1000f;

        RangedAttackServerRpc((int)equippedRarity, targetPoint);
    }

    void TryMeleeAttack()
    {
        if (equippedWeaponType != WeaponType.Sword) return;
        if (Time.time < nextMeleeTime) return;

        nextMeleeTime = Time.time + meleeCooldown;

        if (weaponView != null)
            weaponView.PlaySwordSwing();

        MeleeAttackServerRpc((int)equippedRarity);
    }

    void TryPickup()
    {
        PickupWeaponServerRpc();
    }

    void TryDrop()
    {
        DropWeaponServerRpc();
    }

    public void EquipWeapon(WeaponType type, WeaponRarity rarity)
    {
        equippedWeaponType = type;
        equippedRarity = rarity;

        if (weaponView != null)
        {
            bool swordEquipped = type == WeaponType.Sword;
            weaponView.OnEquipSword(swordEquipped);
        }
    }

    [ClientRpc]
    void EquipWeaponClientRpc(WeaponType type, WeaponRarity rarity)
    {
        EquipWeapon(type, rarity);
    }


    [ServerRpc]
    void RangedAttackServerRpc(int rarityInt, Vector3 targetPoint)
    {
        if (health != null && health.IsDead) return;
        if (equippedWeaponType != WeaponType.Bow) return;
        if (!arrowProjectilePrefab || !bowArrowSpawnPoint) return;

        WeaponRarity rarity = WeaponRarity.Bronze;
        if (WeaponLevelManager.Instance != null)
            rarity = WeaponLevelManager.Instance.GetRarity(WeaponType.Bow);

        Vector3 origin = bowArrowSpawnPoint.position;
        Vector3 dir = (targetPoint - origin).normalized;

        GameObject projObj = Instantiate(arrowProjectilePrefab.gameObject, origin, Quaternion.LookRotation(dir));
        NetworkObject netObj = projObj.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn();

        Projectile projectile = projObj.GetComponent<Projectile>();
        Collider[] myCols = GetComponentsInChildren<Collider>();

        if (projectile != null)
        {
            projectile.SetTargetTeam(HitboxTeam.Enemy);
            int damage = bowBaseDamage * (int)rarity;
            projectile.SetDamage(damage);
            projectile.Init(myCols, OwnerClientId, dir, WeaponType.Bow);
        }
    }

    [ServerRpc]
    void MeleeAttackServerRpc(int rarityInt)
    {
        if (health != null && health.IsDead) return;
        if (equippedWeaponType != WeaponType.Sword) return;

        WeaponRarity rarity = WeaponRarity.Bronze;
        if (WeaponLevelManager.Instance != null)
            rarity = WeaponLevelManager.Instance.GetRarity(WeaponType.Sword);

        Vector3 center = transform.position + transform.forward * meleeRange * 0.5f + Vector3.up;
        Collider[] hits = Physics.OverlapSphere(center, meleeRadius, meleeHitMask, QueryTriggerInteraction.Ignore);
        int damage = swordBaseDamage * (int)rarity;
        int totalDamage = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Hitbox hb = hits[i].GetComponent<Hitbox>();
            if (hb != null && hb.Team == HitboxTeam.Enemy)
            {
                hb.ApplyDamage(damage);
                totalDamage += damage;
            }
        }

        if (totalDamage > 0 && WeaponLevelManager.Instance != null)
            WeaponLevelManager.Instance.AddDamage(WeaponType.Sword, totalDamage);
    }


    [ServerRpc]
    void DropWeaponServerRpc()
    {
        if (health != null && health.IsDead) return;
        if (equippedWeaponType == WeaponType.None) return;

        Vector3 pos = transform.position + transform.forward * dropDistance;
        SpawnPickupForWeapon(equippedWeaponType, pos);

        EquipWeapon(WeaponType.None, WeaponRarity.Bronze);
        EquipWeaponClientRpc(WeaponType.None, WeaponRarity.Bronze);
    }


    [ServerRpc]
    void PickupWeaponServerRpc()
    {
        if (health != null && health.IsDead) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
        WeaponPickup best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            WeaponPickup wp = hits[i].GetComponent<WeaponPickup>();
            if (wp == null) continue;
            float d = (wp.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = wp;
            }
        }

        if (best == null) return;

        WeaponType pickupType = best.Type;
        if (pickupType == WeaponType.None) return;

        if (equippedWeaponType != WeaponType.None && equippedWeaponType != pickupType)
        {
            Vector3 dropPos = transform.position + transform.forward * dropDistance;
            SpawnPickupForWeapon(equippedWeaponType, dropPos);
        }

        WeaponRarity pickupRarity = WeaponRarity.Bronze;
        if (WeaponLevelManager.Instance != null)
            pickupRarity = WeaponLevelManager.Instance.GetRarity(pickupType);

        EquipWeapon(pickupType, pickupRarity);
        EquipWeaponClientRpc(pickupType, pickupRarity);

        NetworkObject no = best.GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned)
            no.Despawn(true);
    }



    void SpawnPickupForWeapon(WeaponType type, Vector3 position)
    {
        NetworkObject prefab = null;
        if (type == WeaponType.Sword) prefab = swordPickupPrefab;
        if (type == WeaponType.Bow) prefab = bowPickupPrefab;
        if (prefab == null) return;

        NetworkObject spawned = Instantiate(prefab, position, Quaternion.identity);
        spawned.Spawn();
    }
}

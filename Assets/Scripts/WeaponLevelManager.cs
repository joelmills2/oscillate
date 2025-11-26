using UnityEngine;
using Unity.Netcode;

public class WeaponLevelManager : NetworkBehaviour
{
    public static WeaponLevelManager Instance;

    [SerializeField] int swordSilverThreshold = 50;
    [SerializeField] int swordGoldThreshold = 100;
    [SerializeField] int bowSilverThreshold = 50;
    [SerializeField] int bowGoldThreshold = 100;

    public NetworkVariable<WeaponRarity> SwordRarity = new NetworkVariable<WeaponRarity>(WeaponRarity.Bronze);
    public NetworkVariable<int> SwordDamage = new NetworkVariable<int>(0);
    public NetworkVariable<WeaponRarity> BowRarity = new NetworkVariable<WeaponRarity>(WeaponRarity.Bronze);
    public NetworkVariable<int> BowDamage = new NetworkVariable<int>(0);

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SwordRarity.Value = WeaponRarity.Bronze;
            SwordDamage.Value = 0;
            BowRarity.Value = WeaponRarity.Bronze;
            BowDamage.Value = 0;
        }
    }

    public void AddDamage(WeaponType type, int amount)
    {
        if (!IsServer) return;
        if (amount <= 0) return;

        if (type == WeaponType.Sword)
        {
            if (SwordRarity.Value == WeaponRarity.Bronze)
            {
                SwordDamage.Value += amount;
                if (SwordDamage.Value >= swordSilverThreshold)
                {
                    SwordRarity.Value = WeaponRarity.Silver;
                    SwordDamage.Value = 0;
                }
            }
            else if (SwordRarity.Value == WeaponRarity.Silver)
            {
                SwordDamage.Value += amount;
                if (SwordDamage.Value >= swordGoldThreshold)
                {
                    SwordRarity.Value = WeaponRarity.Gold;
                    SwordDamage.Value = swordGoldThreshold;
                }
            }
        }
        else if (type == WeaponType.Bow)
        {
            if (BowRarity.Value == WeaponRarity.Bronze)
            {
                BowDamage.Value += amount;
                if (BowDamage.Value >= bowSilverThreshold)
                {
                    BowRarity.Value = WeaponRarity.Silver;
                    BowDamage.Value = 0;
                }
            }
            else if (BowRarity.Value == WeaponRarity.Silver)
            {
                BowDamage.Value += amount;
                if (BowDamage.Value >= bowGoldThreshold)
                {
                    BowRarity.Value = WeaponRarity.Gold;
                    BowDamage.Value = bowGoldThreshold;
                }
            }
        }
    }

    public WeaponRarity GetRarity(WeaponType type)
    {
        if (type == WeaponType.Sword) return SwordRarity.Value;
        if (type == WeaponType.Bow) return BowRarity.Value;
        return WeaponRarity.Bronze;
    }

    public int GetDamage(WeaponType type)
    {
        if (type == WeaponType.Sword) return SwordDamage.Value;
        if (type == WeaponType.Bow) return BowDamage.Value;
        return 0;
    }

    public int GetSilverThreshold(WeaponType type)
    {
        if (type == WeaponType.Sword) return swordSilverThreshold;
        if (type == WeaponType.Bow) return bowSilverThreshold;
        return 1;
    }

    public int GetGoldThreshold(WeaponType type)
    {
        if (type == WeaponType.Sword) return swordGoldThreshold;
        if (type == WeaponType.Bow) return bowGoldThreshold;
        return 1;
    }
}

using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerStatsHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI damageText;

    PlayerCombat combat;
    NetworkObject netObj;

    void Start()
    {
        combat = GetComponentInParent<PlayerCombat>();
        netObj = GetComponentInParent<NetworkObject>();

        if (netObj == null || !netObj.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        int level = 0;
        if (MatchStatsManager.Instance != null)
            level = MatchStatsManager.Instance.EnemyDeathCount.Value;

        int dmg = 0;
        if (combat != null)
            dmg = combat.TotalDamageDealt.Value;

        if (levelText != null)
            levelText.text = "Level: " + level;

        if (damageText != null)
            damageText.text = "Damage: " + dmg;
    }
}
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerWeaponHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI weaponNameText;
    [SerializeField] Slider levelProgressSlider;
    [SerializeField] Image fillImage;
    [SerializeField] Color silverColor = new Color(0.8f, 0.8f, 0.9f);
    [SerializeField] Color goldColor = new Color(1f, 0.85f, 0.3f);

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
        if (combat == null || WeaponLevelManager.Instance == null) return;

        WeaponType type = combat.CurrentWeaponType;

        if (type == WeaponType.None)
        {
            weaponNameText.text = "Unarmed";
            levelProgressSlider.value = 0f;
            fillImage.color = silverColor;
            return;
        }

        WeaponRarity rarity = WeaponLevelManager.Instance.GetRarity(type);

        string name = rarity.ToString() + " " + type.ToString();
        weaponNameText.text = name;

        int damage = WeaponLevelManager.Instance.GetDamage(type);
        int silver = WeaponLevelManager.Instance.GetSilverThreshold(type);
        int gold = WeaponLevelManager.Instance.GetGoldThreshold(type);

        float t = 0f;
        Color nextColor = silverColor;

        if (rarity == WeaponRarity.Bronze)
        {
            t = silver > 0 ? Mathf.Clamp01(damage / (float)silver) : 1f;
            nextColor = silverColor;
        }
        else if (rarity == WeaponRarity.Silver)
        {
            t = gold > 0 ? Mathf.Clamp01(damage / (float)gold) : 1f;
            nextColor = goldColor;
        }
        else
        {
            t = 1f;
            nextColor = goldColor;
        }

        levelProgressSlider.value = t;
        fillImage.color = nextColor;
    }
}

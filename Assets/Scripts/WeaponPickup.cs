using UnityEngine;
using Unity.Netcode;
using TMPro;

public class WeaponPickup : NetworkBehaviour
{
    [SerializeField] WeaponType weaponType;
    [SerializeField] Canvas worldCanvas;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Color bronzeColor = new Color(0.7f, 0.4f, 0.1f);
    [SerializeField] Color silverColor = new Color(0.8f, 0.8f, 0.9f);
    [SerializeField] Color goldColor = new Color(1f, 0.85f, 0.3f);

    public WeaponType Type => weaponType;

    void Update()
    {
        if (!worldCanvas || !label) return;
        if (WeaponLevelManager.Instance == null) return;

        WeaponRarity r = WeaponLevelManager.Instance.GetRarity(weaponType);
        label.text = r.ToString() + " " + weaponType.ToString();

        Color c = bronzeColor;
        if (r == WeaponRarity.Silver) c = silverColor;
        if (r == WeaponRarity.Gold) c = goldColor;
        label.color = c;
    }
}

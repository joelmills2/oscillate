/*
 *  Script adapted from ariel oliveira [o.arielg@gmail.com], found in the Unity Asset Store.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerHeartsHUD : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] PlayerLife life;
    [SerializeField] Transform heartsParent;
    [SerializeField] GameObject heartContainerPrefab;
    [SerializeField] int healthPerHeart = 2;
    [SerializeField] CanvasGroup deathCanvas;
    [SerializeField] TextMeshProUGUI respawnText;

    GameObject[] heartContainers;
    Image[] heartFills;
    NetworkObject netObj;
    float deathTime;
    bool wasDead;

    void Awake()
    {
        if (!health) health = GetComponentInParent<Health>();
        if (!life) life = GetComponentInParent<PlayerLife>();
        netObj = GetComponentInParent<NetworkObject>();

        if (deathCanvas != null)
        {
            deathCanvas.alpha = 0f;
            deathCanvas.blocksRaycasts = false;
            deathCanvas.interactable = false;
        }
    }

    void Start()
    {
        if (netObj != null && !netObj.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        SetupHearts();
    }

    void SetupHearts()
    {
        if (health == null) return;
        if (heartsParent == null) return;
        if (heartContainerPrefab == null) return;

        int max = health.MaxHealth;
        int maxHearts = Mathf.CeilToInt(max / (float)healthPerHeart);
        if (maxHearts <= 0) return;

        heartContainers = new GameObject[maxHearts];
        heartFills = new Image[maxHearts];

        for (int i = 0; i < maxHearts; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab, heartsParent, false);
            heartContainers[i] = temp;
            Transform fillT = temp.transform.Find("HeartFill");
            if (fillT != null)
            {
                heartFills[i] = fillT.GetComponent<Image>();
            }
        }
    }

    void Update()
    {
        if (netObj == null || !netObj.IsOwner) return;
        if (health == null) return;
        if (heartFills == null || heartFills.Length == 0) return;

        UpdateHearts();
        UpdateDeathOverlay();
    }

    void UpdateHearts()
    {
        int max = health.MaxHealth;
        int current = health.CurrentHealth.Value;
        int maxHearts = heartFills.Length;

        for (int i = 0; i < maxHearts; i++)
        {
            int heartStartHealth = i * healthPerHeart;
            if (heartContainers[i] != null)
            {
                bool shouldBeActive = heartStartHealth < max;
                heartContainers[i].SetActive(shouldBeActive);
            }

            if (heartFills[i] == null) continue;

            float value = (current - heartStartHealth) / (float)healthPerHeart;
            value = Mathf.Clamp01(value);
            heartFills[i].fillAmount = value;
        }
    }

    void UpdateDeathOverlay()
    {
        bool dead = health.IsDead;

        if (dead && !wasDead)
        {
            deathTime = Time.time;
            if (deathCanvas != null)
            {
                deathCanvas.alpha = 1f;
                deathCanvas.blocksRaycasts = true;
                deathCanvas.interactable = true;
            }
        }

        if (!dead && wasDead)
        {
            if (deathCanvas != null)
            {
                deathCanvas.alpha = 0f;
                deathCanvas.blocksRaycasts = false;
                deathCanvas.interactable = false;
            }
        }

        if (dead && life != null && respawnText != null)
        {
            float elapsed = Time.time - deathTime;
            float remaining = Mathf.Max(life.RespawnDelay - elapsed, 0f);
            respawnText.text = $"Respawning in {remaining:0.0}";
        }

        wasDead = dead;
    }
}

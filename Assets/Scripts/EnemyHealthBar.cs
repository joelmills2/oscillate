using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image fillImage;
    [SerializeField] bool faceCamera = true;

    Camera cam;

    void Awake()
    {
        if (!health) health = GetComponentInParent<Health>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (health == null || fillImage == null) return;

        float t = 0f;
        if (health.MaxHealth > 0)
            t = Mathf.Clamp01(health.CurrentHealth.Value / (float)health.MaxHealth);
        fillImage.fillAmount = t;

        if (!faceCamera) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}

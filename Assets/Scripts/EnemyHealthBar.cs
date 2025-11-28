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

        fillImage.fillAmount = health.Normalized;

        if (!faceCamera) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}
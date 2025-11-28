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
        cam = Camera.main;
    }

    public void SetFromValues(int current, int max)
    {
        if (!fillImage) return;
        float t = 0f;
        if (max > 0)
            t = Mathf.Clamp01(current / (float)max);
        fillImage.fillAmount = t;
    }

    void LateUpdate()
    {
        if (!faceCamera) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}
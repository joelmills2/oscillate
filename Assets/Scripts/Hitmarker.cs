using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class Hitmarker : MonoBehaviour
{
    public static Hitmarker LocalInstance;

    [SerializeField] Image hitmarkerImage;
    [SerializeField] float visibleTime = 0.1f;
    [SerializeField] float fadeTime = 0.1f;

    NetworkObject netObj;
    Coroutine currentRoutine;

    void Awake()
    {
        netObj = GetComponentInParent<NetworkObject>();

        if (!hitmarkerImage)
            hitmarkerImage = GetComponent<Image>();
    }

    void Start()
    {
        if (netObj == null || !netObj.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        LocalInstance = this;

        if (hitmarkerImage != null)
        {
            Color c = hitmarkerImage.color;
            c.a = 0f;
            hitmarkerImage.color = c;
        }
    }

    public void ShowHitmarker()
    {
        if (hitmarkerImage == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(HitmarkerRoutine());
    }

    IEnumerator HitmarkerRoutine()
    {
        Color c = hitmarkerImage.color;
        c.a = 1f;
        hitmarkerImage.color = c;

        yield return new WaitForSeconds(visibleTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            c.a = a;
            hitmarkerImage.color = c;
            yield return null;
        }

        c.a = 0f;
        hitmarkerImage.color = c;
        currentRoutine = null;
    }
}

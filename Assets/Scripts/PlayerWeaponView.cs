using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerWeaponView : MonoBehaviour
{
    [SerializeField] Transform swordHold;
    [SerializeField] GameObject swordViewPrefab;
    [SerializeField] Vector3 swordLocalPosition = new Vector3(0.2f, -0.3f, 0.6f);
    [SerializeField] Vector3 swordLocalEuler = new Vector3(0f, -40f, 0f);
    [SerializeField] float swingAngle = 45f;
    [SerializeField] float swingDuration = 0.15f;

    NetworkObject netObj;
    GameObject swordInstance;
    Quaternion defaultHoldRotation;
    Coroutine swingRoutine;

    void Awake()
    {
        netObj = GetComponentInParent<NetworkObject>();
    }

    void Start()
    {
        if (netObj == null || !netObj.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        SpawnSword();
    }

    void SpawnSword()
    {
        if (swordViewPrefab == null || swordHold == null) return;
        if (swordInstance != null) return;

        swordInstance = Instantiate(swordViewPrefab, swordHold);
        ApplySwordOffsets();
        defaultHoldRotation = swordHold.localRotation;
        swordInstance.SetActive(false);
    }

    void LateUpdate()
    {
        if (swordInstance != null)
            ApplySwordOffsets();
    }

    void ApplySwordOffsets()
    {
        swordInstance.transform.localPosition = swordLocalPosition;
        swordInstance.transform.localRotation = Quaternion.Euler(swordLocalEuler);
    }

    public void OnEquipSword(bool equipped)
    {
        if (swordInstance == null)
            SpawnSword();

        if (swordInstance != null)
            swordInstance.SetActive(equipped);
    }

    public void PlaySwordSwing()
    {
        if (swordInstance == null || !swordInstance.activeSelf) return;

        if (swingRoutine != null)
            StopCoroutine(swingRoutine);

        swingRoutine = StartCoroutine(SwingRoutine());
    }

    IEnumerator SwingRoutine()
    {
        float t = 0f;

        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / swingDuration);
            float angle = Mathf.Sin(n * Mathf.PI) * swingAngle;
            angle = -angle;
            Quaternion swingRot = Quaternion.Euler(angle, 0f, 0f);
            swordHold.localRotation = defaultHoldRotation * swingRot;
            yield return null;
        }

        swordHold.localRotation = defaultHoldRotation;
        swingRoutine = null;
    }
}

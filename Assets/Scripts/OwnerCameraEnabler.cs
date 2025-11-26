using UnityEngine;
using Unity.Netcode;

public class OwnerCameraEnabler : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private MonoBehaviour[] ownerOnlyScripts;

    public override void OnNetworkSpawn()
    {
        bool isOwner = IsOwner;

        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
            cam.enabled = isOwner;

        foreach (var s in ownerOnlyScripts)
        {
            if (s != null) s.enabled = isOwner;
        }

        if (!isOwner && TryGetComponent<AudioListener>(out var listener))
        {
            listener.enabled = false;
        }
    }
}

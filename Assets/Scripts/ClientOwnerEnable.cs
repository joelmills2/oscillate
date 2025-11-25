/*
Script adapted from: https://www.youtube.com/watch?v=kVt0I6zZsf0
*/

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientOwnerEnable : NetworkBehaviour
{
    [SerializeField] Behaviour cameraController;
    [SerializeField] Camera playerCamera;
    [SerializeField] AudioListener audioListener;

    void Awake()
    {
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>(true);
        if (!audioListener) audioListener = GetComponentInChildren<AudioListener>(true);
        if (!cameraController && playerCamera)
            cameraController = playerCamera.GetComponent<Behaviour>();

        SetLocalControlsEnabled(false);
    }

    void SetLocalControlsEnabled(bool enabled)
    {
        if (cameraController) cameraController.enabled = enabled;
        if (playerCamera) playerCamera.enabled = enabled;
        if (audioListener) audioListener.enabled = enabled;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetLocalControlsEnabled(IsOwner);
    }
}
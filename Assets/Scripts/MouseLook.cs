using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    [SerializeField] float sensitivity = 1.5f;
    [SerializeField] InputActionReference lookAction;

    float yaw;
    float pitch;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        if (IsOwner)
        {
            lookAction.action.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        if (PauseMenu.GameIsPaused) return;

        if (ChatManager.Singleton != null && ChatManager.Singleton.IsTyping)
            return;

        Vector2 look = lookAction.action.ReadValue<Vector2>();

        float deltaYaw = look.x * sensitivity;
        float deltaPitch = look.y * sensitivity;

        yaw += deltaYaw;
        pitch = Mathf.Clamp(pitch - deltaPitch, -90f, 90f);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}

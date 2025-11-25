using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private InputActionReference lookAction;

    private float xRot;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()  => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    void Update()
    {
        if (!IsOwner) return;

        if (PauseMenu.GameIsPaused) return;

        if (ChatManager.Singleton != null && ChatManager.Singleton.IsTyping)
            return;

        Vector2 look = lookAction.action.ReadValue<Vector2>();
        float yaw = look.x * sensitivity * Time.deltaTime;
        float pitch = look.y * sensitivity * Time.deltaTime;

        xRot = Mathf.Clamp(xRot - pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        if (playerBody) playerBody.Rotate(Vector3.up * yaw, Space.World);
    }
}
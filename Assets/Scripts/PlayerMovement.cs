using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float speed = 12f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float groundRadius = 0.3f;
    [SerializeField] float fallMultiplier = 2.5f;

    Rigidbody rb;
    Health health;
    bool jumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (health != null && health.IsDead) return;

        if (jumpAction.action.WasPressedThisFrame())
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (health != null && health.IsDead) return;

        bool grounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (!grounded)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;

        Vector3 v = rb.linearVelocity;
        v.x = dir.x * speed;
        v.z = dir.z * speed;

        if (grounded && jumpPressed)
        {
            v.y = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
        }

        rb.linearVelocity = v;
        jumpPressed = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
}

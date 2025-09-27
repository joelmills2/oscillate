using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float groundRadius = 0.3f;

    [SerializeField] private float fallMultiplier = 2.5f;

    Rigidbody rb;
    bool jumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        if (jumpAction.action.WasPressedThisFrame()) jumpPressed = true;
    }

    void FixedUpdate()
    {
        bool grounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

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
}
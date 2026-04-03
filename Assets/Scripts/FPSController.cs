using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 20f;
    public float airControl = 0.5f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Camera")]
    public Transform cameraPivot;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Rigidbody rb;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalLookRotation;

    private bool isGrounded;

    private Transform currentPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
    }

    void Update()
    {
        HandleLook();
    }

    // ---------------- INPUT SYSTEM (Invoke C# Events) ----------------

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Jump();
        }
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        Vector3 moveDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        float control = isGrounded ? 1f : airControl;

        Vector3 targetVelocity = moveDir * moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - new Vector3(velocity.x, 0, velocity.z));

        rb.AddForce(velocityChange * acceleration * control, ForceMode.Acceleration);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // ---------------- LOOK ----------------

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        cameraPivot.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ---------------- GROUND CHECK ----------------

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Platform detection
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 1f, groundLayer))
        {
            if (hit.transform != currentPlatform)
            {
                currentPlatform = hit.transform;
                transform.SetParent(currentPlatform);
            }
        }
        else
        {
            currentPlatform = null;
            transform.SetParent(null);
        }
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float acceleration = 1.1f;
    [SerializeField] float airControl = 0.5f;

    [Header("Sprint")]
    [SerializeField] float sprintMultiplier = 1.5f;

    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Camera")]
    [SerializeField] Transform cameraPivot;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float maxLookAngle = 80f;

    [Header("Footsteps")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] float baseStepInterval = 0.5f;
    [SerializeField] float sprintStepMultiplier = 0.7f;

    float stepTimer;

    [Header("Jump & Landing Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip softLanding;
    [SerializeField] AudioClip hardLanding;

    bool wasGrounded;

    Rigidbody rb;
    PlayerInputActions input;

    Vector2 moveInput;
    Vector2 lookInput;

    float verticalLookRotation;
    bool isGrounded;
    bool isSprinting;

    System.Action<InputAction.CallbackContext> movePerformed;
    System.Action<InputAction.CallbackContext> moveCanceled;
    System.Action<InputAction.CallbackContext> sprintPerformed;
    System.Action<InputAction.CallbackContext> sprintCanceled;
    System.Action<InputAction.CallbackContext> lookPerformed;
    System.Action<InputAction.CallbackContext> lookCanceled;
    System.Action<InputAction.CallbackContext> jumpPerformed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();

        // move
        movePerformed = ctx => moveInput = ctx.ReadValue<Vector2>();
        moveCanceled = ctx => moveInput = Vector2.zero;
        input.Player.Move.performed += movePerformed;
        input.Player.Move.canceled += moveCanceled;

        // sprint
        sprintPerformed = ctx => isSprinting = true;
        sprintCanceled = ctx => isSprinting = false;
        input.Player.Sprint.performed += sprintPerformed;
        input.Player.Sprint.canceled += sprintCanceled;

        // look
        lookPerformed = ctx => lookInput = ctx.ReadValue<Vector2>();
        lookCanceled = ctx => lookInput = Vector2.zero;
        input.Player.Look.performed += lookPerformed;
        input.Player.Look.canceled += lookCanceled;

        // jump
        jumpPerformed = ctx =>
        {
            if (isGrounded)
                Jump();
        };

        input.Player.Jump.performed += jumpPerformed;
    }

    void OnDisable()
    {
        // move
        input.Player.Move.performed -= movePerformed;
        input.Player.Move.canceled -= moveCanceled;

        // sprint
        input.Player.Sprint.performed -= sprintPerformed;
        input.Player.Sprint.canceled -= sprintCanceled;

        // look
        input.Player.Look.performed -= lookPerformed;
        input.Player.Look.canceled -= lookCanceled;

        // jump
        input.Player.Jump.performed -= jumpPerformed;

        input.Disable();
    }

    void Update()
    {
        HandleLook();
        HandleFootsteps();

        // Unlock with ESC
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorState(false);
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        ApplyBetterJump();
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        float control = isGrounded ? 1f : airControl;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        float speed = (isGrounded && isSprinting && isMoving)
            ? moveSpeed * sprintMultiplier
            : moveSpeed;
        Vector3 targetVelocity = moveDir * speed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, 0, velocity.z);

        rb.AddForce(velocityChange * (acceleration * control), ForceMode.VelocityChange);

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVel.magnitude > speed)
        {
            Vector3 limited = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        PlayJumpSound();
    }

    void PlayJumpSound()
    {
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
    }

    void ApplyBetterJump()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling → faster fall
            rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
        }
        else if (rb.linearVelocity.y > 0 && !Keyboard.current.spaceKey.isPressed)
        {
            // Released jump early → shorter jump
            rb.AddForce(Vector3.down * lowJumpMultiplier, ForceMode.Acceleration);
        }
    }

    // ---------------- LOOK ----------------

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 100f;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        if (cameraPivot == null)
        {
            Debug.LogWarning("Assign Camera Pivot in Player Inspector.");
            return;
        }
        cameraPivot.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        transform.Rotate(transform.up * mouseX);// Use Vector3.up for global rotations
    }

    // ---------------- FOOTSTEPS ----------------

    void HandleFootsteps()
    {
        if (!isGrounded) return;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVel.magnitude < 0.1f) return;

        float speedFactor = horizontalVel.magnitude / moveSpeed;

        float interval = baseStepInterval;

        if (isSprinting)
            interval *= sprintStepMultiplier;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = interval / Mathf.Max(speedFactor, 0.1f);
        }
    }

    void PlayFootstep()
    {
        footstepSource.Play();
    }

    // ---------------- GROUND CHECK ----------------

    void CheckGround()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("Assign Ground Check in Player Inspector.");
            return;
        }

        bool previousGrounded = isGrounded;

        // If you need the RaycastHit info then use SphereCast instead of CheckSphere
        isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundCheckRadius,
        groundLayer);

        // Landing detection
        if (!previousGrounded && isGrounded)
        {
            PlayLandingSound();
        }
    }

    void PlayLandingSound()
    {
        float fallSpeed = Mathf.Abs(rb.linearVelocity.y);

        if (fallSpeed < 2f) return;

        AudioClip clip = fallSpeed > 8f ? hardLanding : softLanding;
        audioSource.PlayOneShot(clip);
    }

    // ---------------- CURSOR LOGIC (TEMPORARY) ----------------

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) SetCursorState(true);
    }

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // ---------------- DEBUG ----------------

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        // Draw one sphere at the groundCheck position
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
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

    float yaw;
    public float GetYaw() => yaw;

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
        Vector3 flatVelocity = Vector3.ProjectOnPlane(velocity, transform.up);

        Vector3 velocityChange = targetVelocity - flatVelocity;

        rb.AddForce(velocityChange * (acceleration * control), ForceMode.VelocityChange);

        Vector3 up = transform.up;
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.linearVelocity, up);

        if (flatVel.magnitude > speed)
        {
            Vector3 limited = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        PlayJumpSound();
    }

    void PlayJumpSound()
    {
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
    }

    void ApplyBetterJump()
    {
        float verticalSpeed = Vector3.Dot(rb.linearVelocity, transform.up);
        if (verticalSpeed < 0)
        {
            rb.AddForce(-transform.up * fallMultiplier, ForceMode.Acceleration);
        }
        else if (verticalSpeed > 0 && !Keyboard.current.spaceKey.isPressed)
        {
            rb.AddForce(-transform.up * lowJumpMultiplier, ForceMode.Acceleration);
        }
    }

    // ---------------- LOOK ----------------

    void HandleLook()
    {
        Vector2 input = lookInput;

        // Clamp diagonal speed
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        float mouseX = input.x * mouseSensitivity;
        float mouseY = input.y * mouseSensitivity;

        yaw += mouseX;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        if (cameraPivot == null)
        {
            Debug.LogWarning("Assign Camera Pivot in Player Inspector.");
            return;
        }

        Quaternion targetRot = Quaternion.AngleAxis(verticalLookRotation, Vector3.right);

        cameraPivot.localRotation = Quaternion.Slerp(
            cameraPivot.localRotation,
            targetRot,
            15f * Time.deltaTime
        );

        Vector3 euler = cameraPivot.localEulerAngles;
        cameraPivot.localRotation = Quaternion.Euler(euler.x, 0f, 0f);
    }

    // ---------------- FOOTSTEPS ----------------

    void HandleFootsteps()
    {
        if (!isGrounded) return;

        Vector3 up = transform.up;

        // Surface-relative movement
        Vector3 horizontalVel = Vector3.ProjectOnPlane(rb.linearVelocity, up);

        float speed = horizontalVel.magnitude;

        if (speed < 0.1f) return;

        float speedFactor = speed / moveSpeed;

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
        float fallSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.up));

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
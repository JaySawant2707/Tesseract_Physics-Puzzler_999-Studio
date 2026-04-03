using UnityEngine;

/// <summary>
/// Applies custom gravity to a Rigidbody and rotates the transform so its up axis
/// matches the smoothed surface normal. Built for players but reusable for any body.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GravitySurfaceDetector))]
public sealed class CustomGravityBody : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravityForce = 30f;
    [SerializeField] private float normalLerpSpeed = 10f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Surface Stick")]
    [SerializeField] private bool stickToSurface = true;
    [SerializeField] private float stickForce = 20f;
    [SerializeField] private float stickDistance = 0.75f;

    [Header("References (optional overrides)")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private GravitySurfaceDetector detector;
    [SerializeField] private FPSController controller;

    // This represents "up" for the object. Gravity force is applied in the opposite direction.
    private Vector3 currentGravityDirection = Vector3.up;
    private bool loggedMissingReference;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnValidate()
    {
        if (gravityForce < 0f) gravityForce = 0f;
        if (normalLerpSpeed < 0f) normalLerpSpeed = 0f;
        if (rotationSpeed < 0f) rotationSpeed = 0f;
        if (stickForce < 0f) stickForce = 0f;
        if (stickDistance < 0f) stickDistance = 0f;

        if (body == null) body = GetComponent<Rigidbody>();
        if (detector == null) detector = GetComponent<GravitySurfaceDetector>();
    }

    private void OnEnable()
    {
        EnsureInitialized();
    }

    private void FixedUpdate()
    {
        if (!EnsureInitialized())
        {
            return;
        }

        detector.Probe(currentGravityDirection);

        Vector3 targetNormal = currentGravityDirection;

        if (detector.HasSurface)
        {
            if (Vector3.Angle(currentGravityDirection, detector.TargetNormal) > 2f)
            {
                targetNormal = detector.TargetNormal;
            }
        }
        // Smoothly blend current gravity-up direction to target surface normal for edge transitions.
        float normalBlend = 1f - Mathf.Exp(-normalLerpSpeed * Time.fixedDeltaTime);
        currentGravityDirection = Vector3.Slerp(currentGravityDirection, targetNormal, normalBlend).normalized;

        // Apply gravity opposite to the smoothed "up" direction.
        body.AddForce(-currentGravityDirection * gravityForce, ForceMode.Acceleration);

        if (stickToSurface && detector.HasSurface && detector.SurfaceDistance <= stickDistance)
        {
            // Extra downward force to reduce micro-floating near edges/uneven terrain.
            body.AddForce(-currentGravityDirection * stickForce, ForceMode.Acceleration);
        }

        AlignToGravityUp();
    }

    private void AlignToGravityUp()
    {
        Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, currentGravityDirection);

        // Prevent zero vector (happens when looking straight up/down)
        if (forwardProjected.sqrMagnitude < 0.001f)
        {
            forwardProjected = Vector3.ProjectOnPlane(transform.right, currentGravityDirection);
        }

        float yaw = controller != null ? controller.GetYaw() : 0f;

        // Build rotation around current gravity up
        Quaternion yawRotation = Quaternion.AngleAxis(yaw, currentGravityDirection);

        // Forward direction based on yaw
        Vector3 forward = yawRotation * Vector3.forward;

        // Project onto surface
        forward = Vector3.ProjectOnPlane(forward, currentGravityDirection).normalized;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(yawRotation * Vector3.right, currentGravityDirection);
        }

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(transform.right, currentGravityDirection);

        Quaternion targetRotation = Quaternion.LookRotation(forward, currentGravityDirection);

        float rotationBlend = 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime);

        Quaternion smoothedRotation = Quaternion.Slerp(body.rotation, targetRotation, rotationBlend);

        body.MoveRotation(smoothedRotation);
    }

    /// <summary>
    /// Exposes current custom up direction to other systems (camera, movement, VFX).
    /// </summary>
    public Vector3 GetUpDirection() => currentGravityDirection;

    /// <summary>
    /// Exposes current down/gravity pull direction.
    /// </summary>
    public Vector3 GetDownDirection() => -currentGravityDirection;

    private bool EnsureInitialized()
    {
        if (controller == null) controller = GetComponent<FPSController>();
        if (body == null) body = GetComponent<Rigidbody>();
        if (detector == null) detector = GetComponent<GravitySurfaceDetector>();

        if (body == null || detector == null)
        {
            if (!loggedMissingReference)
            {
                Debug.LogError(
                    $"{nameof(CustomGravityBody)} on '{name}' requires both {nameof(Rigidbody)} and {nameof(GravitySurfaceDetector)} components.",
                    this
                );
                loggedMissingReference = true;
            }
            return false;
        }

        body.useGravity = false;

        if (currentGravityDirection == Vector3.zero)
        {
            currentGravityDirection = transform.up;
        }

        return true;
    }
}

using UnityEngine;

/// <summary>
/// Detects nearby surfaces and exposes a target normal for custom gravity systems.
/// Split out as a reusable component so any Rigidbody can share detection logic.
/// </summary>
[DisallowMultipleComponent]
public sealed class GravitySurfaceDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float sphereRadius = 0.35f;
    [SerializeField] private float detectionDistance = 1.5f;
    [SerializeField] private float castStartOffset = 0.15f;
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    public bool HasSurface { get; private set; }
    public Vector3 TargetNormal { get; private set; } = Vector3.up;
    public float SurfaceDistance { get; private set; }

    /// <summary>
    /// Performs a SphereCast from object's up axis toward its down axis.
    /// Uses only value types in hot path (no managed allocations).
    /// </summary>
    public void Probe(Vector3 currentUp)
    {
        Vector3 castOrigin = transform.position + (currentUp * castStartOffset);
        Vector3 castDirection = -currentUp;

        bool hitSurface = Physics.SphereCast(
            castOrigin,
            sphereRadius,
            castDirection,
            out RaycastHit hit,
            detectionDistance,
            surfaceMask,
            triggerInteraction
        );

        if (hitSurface)
        {
            HasSurface = true;
            TargetNormal = hit.normal;
            SurfaceDistance = hit.distance;
        }
        else
        {
            HasSurface = false;
            SurfaceDistance = detectionDistance;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = HasSurface ? Color.green : Color.yellow;

        Vector3 up = transform.up;
        Vector3 castOrigin = transform.position + (up * castStartOffset);
        Vector3 castEnd = castOrigin - (up * detectionDistance);

        Gizmos.DrawWireSphere(castOrigin, sphereRadius);
        Gizmos.DrawLine(castOrigin, castEnd);
        Gizmos.DrawWireSphere(castEnd, sphereRadius);

        if (HasSurface)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(castEnd, TargetNormal * 0.6f);
        }
    }
#endif
}

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

    private Rigidbody body;
    private GravitySurfaceDetector detector;

    // This represents "up" for the object. Gravity force is applied in the opposite direction.
    private Vector3 currentGravityDirection = Vector3.up;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        detector = GetComponent<GravitySurfaceDetector>();

        // Disable built-in gravity (required for custom directional gravity).
        body.useGravity = false;

        currentGravityDirection = transform.up;
    }

    private void OnValidate()
    {
        if (gravityForce < 0f) gravityForce = 0f;
        if (normalLerpSpeed < 0f) normalLerpSpeed = 0f;
        if (rotationSpeed < 0f) rotationSpeed = 0f;
        if (stickForce < 0f) stickForce = 0f;
        if (stickDistance < 0f) stickDistance = 0f;
    }

    private void FixedUpdate()
    {
        detector.Probe(currentGravityDirection);

        Vector3 targetNormal = detector.HasSurface ? detector.TargetNormal : currentGravityDirection;

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
        Quaternion currentRotation = body.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, currentGravityDirection) * currentRotation;

        float rotationBlend = 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime);
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationBlend);

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
}

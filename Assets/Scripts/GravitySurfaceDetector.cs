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
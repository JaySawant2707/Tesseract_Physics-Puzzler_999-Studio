using UnityEngine;

public class CameraFollowNoRotation : MonoBehaviour
{
    public Transform target;       // Player
    public Vector3 offset = new Vector3(0, 5, -10); // Global offset
    public float smoothSpeed = 5f; // Optional smoothing

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position = player's position + FIXED global offset
        Vector3 desiredPosition = target.position + offset;

        // Smooth movement (optional but recommended)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        // IMPORTANT: No rotation following
        // Keep camera rotation fixed (do nothing here)
    }
}
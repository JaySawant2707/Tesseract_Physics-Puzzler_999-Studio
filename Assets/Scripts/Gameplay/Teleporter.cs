using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform destination;
    public Teleporter destinationTeleporter;

    public bool matchRotation = true;
    public bool keepVelocity = true;

    [Header("Cooldown")]
    public float teleportCooldown = 0.2f;

    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canTeleport) return;

        if (other.CompareTag("Player"))
        {
            Teleport(other);
        }
    }

    void Teleport(Collider player)
    {
        // Disable BOTH portals
        canTeleport = false;
        if (destinationTeleporter != null)
            destinationTeleporter.canTeleport = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        Vector3 velocity = Vector3.zero;
        if (rb != null && keepVelocity)
            velocity = rb.linearVelocity;

        // Move player
        player.transform.position = destination.position;

        // Match rotation
        if (matchRotation)
            player.transform.rotation = destination.rotation;

        // Restore velocity
        if (rb != null && keepVelocity)
            rb.linearVelocity = velocity;

        // Re-enable after delay
        Invoke(nameof(ResetTeleport), teleportCooldown);
        if (destinationTeleporter != null)
            destinationTeleporter.Invoke(nameof(ResetTeleport), teleportCooldown);

        Vector3 exitOffset = destination.forward * 1.5f;
        player.transform.position = destination.position + exitOffset;
    }

    void ResetTeleport()
    {
        canTeleport = true;
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Plug : MonoBehaviour, IInteractable
{
    public string key;
    bool isMovingToHand = false;
    Transform targetHoldPoint;

    [SerializeField] float pickupSpeed = 10f;
    [SerializeField] float rotateSpeed = 12f;

    private Rigidbody rb;
    private Collider col;
    bool isPlugged = false;
    int originalLayer;
    float pickupTimer = 0f;
    bool pickupRequested = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        if (pickupRequested)
        {
            pickupRequested = false;

            PlayerHoldSystem player = FindFirstObjectByType<PlayerHoldSystem>();

            if (player != null)
            {
                player.PickUp(this);
            }
        }

        if (isMovingToHand)
        {
            pickupTimer += Time.deltaTime;
            MoveToHand();

            if (pickupTimer > 1f)
            {
                ForceAttach();
            }
        }
    }

    void ForceAttach()
    {
        transform.SetParent(targetHoldPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        isMovingToHand = false;
        pickupTimer = 0f;
    }

    void MoveToHand()
    {
        // Smooth position (CLEAN LERP)
        transform.position = Vector3.Lerp(
            transform.position,
            targetHoldPoint.position,
            pickupSpeed * Time.deltaTime
        );

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetHoldPoint.rotation,
            rotateSpeed * Time.deltaTime
        );

        // Snap when close enough
        if (Vector3.Distance(transform.position, targetHoldPoint.position) < 0.1f)
        {
            transform.SetParent(targetHoldPoint);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            pickupTimer = 0f;
            isMovingToHand = false;
        }
    }

    public void Interact()
    {
        PlayerHoldSystem player = FindFirstObjectByType<PlayerHoldSystem>();

        if (player.CanPickUp() && !IsBusy())
        {
            // ✅ Step 1: disable collision BEFORE physics
            gameObject.layer = LayerMask.NameToLayer("HeldObject");

            // ✅ Step 2: delay pickup to next frame ONLY
            pickupRequested = true;

            rb.excludeLayers = LayerMask.GetMask("Player");
        }
    }

    public void OnPicked(Transform holdPoint)
    {
        
        targetHoldPoint = holdPoint;

        // 🔥 FORCE move OUTSIDE player collider (IMPORTANT)
        Vector3 forward = holdPoint.forward;
        transform.position = holdPoint.position + forward * 0.7f;

        // Reset physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        col.enabled = false;

        isMovingToHand = true;

        if (TryGetComponent<FloatingObject>(out var floating))
            floating.OnPicked();
    }

    public void OnPlugged()
    {
        gameObject.layer = originalLayer;
        rb.excludeLayers = 0;
        isPlugged = true;

        if (TryGetComponent<FloatingObject>(out var floating))
            floating.DisableFloating();
    }

    public void OnDropped()
    {
        gameObject.layer = originalLayer;
        rb.excludeLayers = 0;

        if (isPlugged) return;

        rb.isKinematic = false;
        col.enabled = true;

        transform.SetParent(null);

        if (TryGetComponent<FloatingObject>(out var floating))
            floating.OnDropped();
    }

    public bool IsBusy()
    {
        return isMovingToHand;
    }
}
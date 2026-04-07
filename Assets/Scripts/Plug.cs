using UnityEngine;

public class Plug : MonoBehaviour, IInteractable
{
    public string key;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Interact()
    {
        PlayerHoldSystem player = FindFirstObjectByType<PlayerHoldSystem>();

        if (player.CanPickUp())
        {
            player.PickUp(this);
        }
    }

    public void OnPicked(Transform holdPoint)
    {
        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(holdPoint, false); // ✅ FIX

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDropped()
    {
        rb.isKinematic = false;
        col.enabled = true;

        transform.SetParent(null);
    }
}
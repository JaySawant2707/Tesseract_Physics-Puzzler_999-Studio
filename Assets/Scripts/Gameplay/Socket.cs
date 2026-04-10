using UnityEngine;
using UnityEngine.Events;

public class Socket : MonoBehaviour, IInteractable
{
    public string key;
    public Transform snapPoint;
    public UnityEvent onConnected;

    private bool isOccupied = false;

    public void Interact()
    {
        PlayerHoldSystem player = FindFirstObjectByType<PlayerHoldSystem>();

        if (!player.HasObject()) return;

        Plug heldPlug = player.GetHeldPlug();

        if (heldPlug.key == key && !isOccupied)
        {
            PlacePlug(heldPlug);
            player.ClearHeld();
        }
    }

    void PlacePlug(Plug plug)
    {
        isOccupied = true;

        plug.transform.SetParent(null);
        plug.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
        plug.GetComponent<Rigidbody>().isKinematic = true;

        plug.OnPlugged();

        onConnected?.Invoke();
    }
}
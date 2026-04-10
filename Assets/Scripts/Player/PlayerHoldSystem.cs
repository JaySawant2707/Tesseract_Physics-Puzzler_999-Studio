using UnityEngine;

public class PlayerHoldSystem : MonoBehaviour
{
    public Transform holdPoint;
    public Collider playerCollider;

    private Plug currentPlug;

    public bool CanPickUp()
    {
        return currentPlug == null;
    }

    public bool HasObject()
    {
        return currentPlug != null;
    }

    public Plug GetHeldPlug()
    {
        return currentPlug;
    }

    public void PickUp(Plug plug)
    {
        if (currentPlug != null) return;

        currentPlug = plug;
        plug.OnPicked(holdPoint);
    }   

    public void ClearHeld()
    {
        if (currentPlug != null)
        {
            currentPlug.OnDropped();
            currentPlug = null;
        }
    }
}
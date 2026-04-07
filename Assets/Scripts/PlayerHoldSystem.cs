using UnityEngine;

public class PlayerHoldSystem : MonoBehaviour
{
    public Transform holdPoint;

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
        currentPlug = plug;
        plug.OnPicked(holdPoint);
    }

    public void ClearHeld()
    {
        currentPlug = null;
    }
}
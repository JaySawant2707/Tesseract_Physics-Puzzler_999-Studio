using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 4f;
    public LayerMask interactLayer;

    [Header("References")]
    public Camera playerCamera;

    IInteractable currentInteractable;
    Outline currentOutline;

    void Update()
    {
        DetectInteractable();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }

    void DetectInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    ClearCurrent();

                    currentInteractable = interactable;

                    currentOutline = hit.collider.GetComponent<Outline>();
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = true;
                    }
                }

                return;
            }
        }

        ClearCurrent();
    }

    void ClearCurrent()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        currentInteractable = null;
        currentOutline = null;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 4f;
    public LayerMask interactLayer;

    IInteractable currentInteractable;
    Outline currentOutline;
    public bool isBlocked;

    void Update()
    {
        if (isBlocked) return;
        
        DetectInteractable();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            PlayerHoldSystem player = FindFirstObjectByType<PlayerHoldSystem>();

            // If looking at interactable → use it (socket, etc.)
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
                return;
            }

            // Otherwise → drop if holding
            if (player != null && player.HasObject())
            {
                player.ClearHeld();
            }
        }
    }

    void DetectInteractable()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
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
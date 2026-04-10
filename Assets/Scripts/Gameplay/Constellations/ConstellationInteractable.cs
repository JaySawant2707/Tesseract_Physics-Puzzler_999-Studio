using Unity.Cinemachine;
using UnityEngine;

public class ConstellationInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    public ConstellationPuzzle puzzle;
    public FPSController playerController; // from your file :contentReference[oaicite:0]{index=0}
    public PlayerInteraction interaction;


public CinemachineCamera puzzleCam;
public CinemachineCamera playerCam;

private bool isActive = false;

    public void Interact()
    {
        if (isActive) return;

        StartPuzzle();
    }

    void StartPuzzle()
    {
        isActive = true;

        playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 🎥 Switch camera
        puzzleCam.Priority = 20;
        playerCam.Priority = 10;

        puzzle.Activate();
        puzzle.OnSolved += OnPuzzleSolved;
    }

    void OnPuzzleSolved()
    {
        puzzle.OnSolved -= OnPuzzleSolved;

        ExitPuzzle();

        Debug.Log("Puzzle Completed → Trigger something here");
    }

    void ExitPuzzle()
    {
        isActive = false;

        playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🎥 Switch back
        puzzleCam.Priority = 5;
        playerCam.Priority = 10;

        puzzle.Deactivate();
    }
}
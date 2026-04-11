using System;
using UnityEngine;

public class CubePlacementManager : MonoBehaviour
{
    // Singleton instance
    public static CubePlacementManager Instance { get; private set; }

    // Booleans
    private bool hasPlacedCube1;
    private bool hasPlacedCube2;
    private bool hasPlacedCube3;
    private bool hasPlacedCube4;

    // Event triggered when all are true
    public event Action OnAllCubesPlaced;

    private bool eventInvoked = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Public method to set cube placement
    public void SetCubePlaced(int cubeIndex, bool value)
    {
        switch (cubeIndex)
        {
            case 1: hasPlacedCube1 = value; break;
            case 2: hasPlacedCube2 = value; break;
            case 3: hasPlacedCube3 = value; break;
            case 4: hasPlacedCube4 = value; break;
            default:
                Debug.LogWarning("Invalid cube index");
                return;
        }

        CheckAllPlaced();
    }

    // Optional getters
    public bool AreAllCubesPlaced()
    {
        return hasPlacedCube1 && hasPlacedCube2 && hasPlacedCube3 && hasPlacedCube4;
    }

    private void CheckAllPlaced()
    {
        if (!eventInvoked && AreAllCubesPlaced())
        {
            eventInvoked = true;
            OnAllCubesPlaced?.Invoke();
        }
    }
}
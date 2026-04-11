using UnityEngine;

public class WorldCubes : MonoBehaviour
{
    public static WorldCubes Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Cube ownership flags
    public bool hasCube1;
    public bool hasCube2;
    public bool hasCube3;
    public bool hasCube4;

    // Cube prefabs (assign in Inspector)
    public GameObject cube1Prefab;
    public GameObject cube2Prefab;
    public GameObject cube3Prefab;
    public GameObject cube4Prefab;

    // -------------------------
    // SETTERS (BOOL ONLY)
    // -------------------------
    public void SetCube1(bool value)
    {
        hasCube1 = value;
    }

    public void SetCube2(bool value)
    {
        hasCube2 = value;
    }

    public void SetCube3(bool value)
    {
        hasCube3 = value;
    }

    public void SetCube4(bool value)
    {
        hasCube4 = value;
    }

    // Optional getters
    public bool HasCube1() => hasCube1;
    public bool HasCube2() => hasCube2;
    public bool HasCube3() => hasCube3;
    public bool HasCube4() => hasCube4;

    // -------------------------
    // SPAWN LOGIC (WITH PARAMS)
    // -------------------------
    public void OnSceneStartSpawn(
        Transform spawn1,
        Transform spawn2,
        Transform spawn3,
        Transform spawn4)
    {
        if (hasCube1 && cube1Prefab && spawn1)
            Instantiate(cube1Prefab, spawn1.position, spawn1.rotation);

        if (hasCube2 && cube2Prefab && spawn2)
            Instantiate(cube2Prefab, spawn2.position, spawn2.rotation);

        if (hasCube3 && cube3Prefab && spawn3)
            Instantiate(cube3Prefab, spawn3.position, spawn3.rotation);

        if (hasCube4 && cube4Prefab && spawn4)
            Instantiate(cube4Prefab, spawn4.position, spawn4.rotation);
    }
}
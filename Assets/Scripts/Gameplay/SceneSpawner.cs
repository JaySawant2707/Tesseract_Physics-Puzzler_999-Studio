using UnityEngine;

public class SceneSpawner : MonoBehaviour
{
    public Transform spawn1;
    public Transform spawn2;
    public Transform spawn3;
    public Transform spawn4;

    void Start()
    {
        WorldCubes.Instance.OnSceneStartSpawn(spawn1, spawn2, spawn3, spawn4);
    }
}
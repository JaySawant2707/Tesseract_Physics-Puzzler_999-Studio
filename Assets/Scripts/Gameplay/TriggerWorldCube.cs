using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerWorldCube : MonoBehaviour
{
    [SerializeField] int index;
    [SerializeField] int cubeNum;
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (cubeNum)
            {
                case 1:
                    WorldCubes.Instance.SetCube1(true);
                    break;
                case 2:
                    WorldCubes.Instance.SetCube2(true);
                    break;
                case 3:
                    WorldCubes.Instance.SetCube3(true);
                    break;
                default:
                    WorldCubes.Instance.SetCube4(true);
                    break;
            }
            SceneManager.LoadScene(index);
        }
    }
}
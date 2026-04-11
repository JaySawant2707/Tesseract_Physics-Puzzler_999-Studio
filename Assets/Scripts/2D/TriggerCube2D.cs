using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerCube2D : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            WorldCubes.Instance.SetCube1(true);
            SceneManager.LoadScene(1);
        }
    }
}
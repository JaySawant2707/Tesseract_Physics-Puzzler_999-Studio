using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class RestartBtn : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Restart();
        }
    }
}
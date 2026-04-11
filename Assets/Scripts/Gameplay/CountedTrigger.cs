using UnityEngine;
using UnityEngine.Events;

public class CountedTrigger : MonoBehaviour
{
    public int countNumber = 4;
    public UnityEvent onActivated;
    int count;

    public void IncreaseCounter()
    {
        count++;
        CheckSuccess();
    }

    void CheckSuccess()
    {
        if (count >= countNumber)
        {
            onActivated?.Invoke();
        }
    }
}

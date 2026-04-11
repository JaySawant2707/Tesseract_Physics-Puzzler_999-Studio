using UnityEngine;
using UnityEngine.Events;

public class CountedTrigger : MonoBehaviour
{
    public int countNumber = 4;
    public UnityEvent onActivated;
    int count;

    public void IncreaseCounter(int indx)
    {
        count++;
        CubePlacementManager.Instance.SetCubePlaced(indx, true);
        CheckSuccess();
    }

    public void IncreaseCounter()
    {
        count++;
        CheckSuccess2();
    }

    void CheckSuccess()
    {
        if (CubePlacementManager.Instance.AreAllCubesPlaced())
        {
            onActivated?.Invoke();
        }
    }

    void CheckSuccess2()
    {
        if (count >= countNumber)
        {
            onActivated?.Invoke();
        }
    }
}

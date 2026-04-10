using UnityEngine;

public class Bridge : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void BuildBridge()
    {
        animator.SetTrigger("Build");
    }
}

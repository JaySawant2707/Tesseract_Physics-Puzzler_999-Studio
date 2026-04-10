using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] Camera camera2d;
    void LateUpdate()
    {
        // Option A: Spherical (Rotates on all axes to face camera)
        transform.LookAt(camera2d.transform.position);

        // Option B: Cylindrical (Keeps object upright, only rotates on Y-axis)
        // Vector3 targetPos = Camera.main.transform.position;
        // targetPos.y = transform.position.y;
        // transform.LookAt(targetPos);
    }
}

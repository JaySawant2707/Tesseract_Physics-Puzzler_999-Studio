using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] points;

    [Tooltip("Movement speed (units per second)")]
    public float moveSpeed = 3f;

    [Tooltip("How close the object must be to switch to next point")]
    public float reachDistance = 0.1f;

    [Tooltip("Loop back to first point after last")]
    public bool loop = true;

    [Tooltip("Move back and forth instead of looping")]
    public bool pingPong = false;

    [Tooltip("Start moving on Start")]
    public bool canMove = true;

    [Header("Rotation Settings")]
    [Tooltip("Should the object face the movement direction")]
    public bool faceForward = false;

    [Tooltip("Rotation speed (0 = instant rotation)")]
    public float rotationSpeed = 5f;


    private int currentIndex = 0;
    private int direction = 1;

    void Start()
    {
        if (points != null && points.Length > 0)
            transform.position = points[0].position;
    }

    void Update()
    {
        if (points == null || points.Length == 0 || !canMove)
            return;

        Transform target = points[currentIndex];

        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Face movement direction
        if (faceForward)
        {
            FaceTarget(target.position);
        }

        // Check if reached target
        if (Vector3.Distance(transform.position, target.position) <= reachDistance)
        {
            MoveToNextPoint();
        }
    }


    void MoveToNextPoint()
    {
        if (pingPong)
        {
            if (currentIndex == points.Length - 1)
                direction = -1;
            else if (currentIndex == 0)
                direction = 1;

            currentIndex += direction;
        }
        else
        {
            currentIndex++;

            if (currentIndex >= points.Length)
            {
                if (loop)
                    currentIndex = 0;
                else
                    currentIndex = points.Length - 1;
            }
        }
    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (rotationSpeed <= 0f)
        {
            transform.rotation = targetRotation; // instant
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }


    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i] == null || points[i + 1] == null)
                continue;

            Gizmos.DrawLine(points[i].position, points[i + 1].position);
            Gizmos.DrawSphere(points[i].position, 0.15f);
        }

        // Draw last point sphere
        if (points[points.Length - 1] != null)
        {
            Gizmos.DrawSphere(points[points.Length - 1].position, 0.15f);
        }

        // Loop line (optional)
        if (loop && !pingPong && points[0] != null && points[points.Length - 1] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(points[points.Length - 1].position, points[0].position);
        }
    }

    public void CanMove(bool value)
    {
        canMove = value;
    }
}
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] float floatHeight = 0.5f;
    [SerializeField] float floatSpeed = 1.5f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 20f;

    [Header("Settings")]
    [SerializeField] Transform visual;
    [SerializeField] bool useLocalUp = true;

    float timeOffset;

    bool isActive = true;
    bool isHeld = false;

    Vector3 initialLocalPos;
    Quaternion initialLocalRot;

    void Start()
    {
        timeOffset = Random.Range(0f, 100f);

        if (visual != null)
        {
            initialLocalPos = visual.localPosition;
            initialLocalRot = visual.localRotation;
        }
    }

    void Update()
    {
        if (!isActive || isHeld || visual == null) return;

        Float();
    }

    void LateUpdate()
    {
        if (!isActive || isHeld || visual == null) return;

        Rotate();
    }

    void Float()
    {
        Vector3 up = useLocalUp ? transform.up : Vector3.up;

        float wave = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;

        // ✅ MOVE ONLY CHILD (local space)
        visual.localPosition = initialLocalPos + up * wave;
    }

    void Rotate()
    {
        Vector3 up = useLocalUp ? transform.up : Vector3.up;

        // ✅ ROTATE ONLY CHILD
        visual.Rotate(up, rotationSpeed * Time.deltaTime, Space.World);
    }

    // 🔌 Called when socket is activated
    public void DisableFloating()
    {
        isActive = false;
        ResetVisual();
    }

    public void EnableFloating()
    {
        isActive = true;
    }

    // 🤚 Called when picked
    public void OnPicked()
    {
        isHeld = true;
        ResetVisual(); // 🔥 IMPORTANT
    }

    // 🫳 Called when dropped
    public void OnDropped()
    {
        isHeld = false;
    }

    void ResetVisual()
    {
        if (visual == null) return;

        visual.localPosition = initialLocalPos;
        visual.localRotation = initialLocalRot;
    }
}
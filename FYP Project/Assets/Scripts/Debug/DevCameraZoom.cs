using UnityEngine;

public class DevCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform focusTarget;

    [Header("Zoom")]
    public float zoomSpeed = 20f;
    public float minDistance = 5f;
    public float maxDistance = 40f;

    [Header("Rotation")]
    public float rotationSpeed = 120f;
    public float minPitch = 20f;
    public float maxPitch = 80f;

    [Header("Pan")]
    public float panSpeed = 10f;
    public float fastPanMultiplier = 2f;

    private float currentDistance = 15f;
    private float yaw = 45f;
    private float pitch = 45f;

    void Start()
    {
        if (focusTarget == null)
        {
            Debug.LogWarning("DevCameraController: No focus target assigned.");
            enabled = false;
            return;
        }

        Vector3 offset = transform.position - focusTarget.position;
        currentDistance = offset.magnitude;

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        UpdateCameraPosition();
    }

    void Update()
    {
        if (focusTarget == null)
            return;

        HandleZoom();
        HandleRotation();
        HandlePan();

        UpdateCameraPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    void HandleRotation()
    {
        // Middle mouse drag rotation
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
        }

        // Optional keyboard rotation
        if (Input.GetKey(KeyCode.Q))
        {
            yaw -= rotationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            yaw += rotationSpeed * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandlePan()
    {
        float currentPanSpeed = panSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentPanSpeed *= fastPanMultiplier;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.D))
            horizontal = 1f;
        if (Input.GetKey(KeyCode.W))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S))
            vertical = -1f;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * vertical + right * horizontal) * currentPanSpeed * Time.deltaTime;

        focusTarget.position += move;
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -currentDistance);

        transform.position = focusTarget.position + offset;
        transform.rotation = rotation;
    }
}
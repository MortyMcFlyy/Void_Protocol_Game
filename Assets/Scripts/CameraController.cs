using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 thirdPersonOffset = new Vector3(0.5f, 0.5f, -1f);
    public Vector3 lookOffset = new Vector3(0.5f, 0.5f, 0f);
    public float lookAtHeight = 0.5f;

    [Header("Smoothness & Collision")]
    public float smoothSpeed = 0.2f;
    public float sphereRadius = 0.3f;
    public float minDistance = 0.2f;
    public LayerMask obstacleMask;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 70f;

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 lookTarget;
    private Vector3 finalCameraPosition;

    private Vector3 targetPosition;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    void LateUpdate()
    {
        targetPosition = target.position;
        targetPosition.y += lookAtHeight;

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredCameraPosition = targetPosition + cameraRotation * thirdPersonOffset;
        lookTarget = targetPosition + cameraRotation * lookOffset;

        Vector3 direction = desiredCameraPosition - lookTarget;
        float maxDistance = direction.magnitude;

        finalCameraPosition = desiredCameraPosition;

        if (direction.magnitude > 0.01f && Physics.SphereCast(lookTarget, sphereRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleMask))
        {
            float safeDistance = Mathf.Max(minDistance, hit.distance - 0.05f);
            finalCameraPosition = lookTarget + direction.normalized * safeDistance;
        }

        transform.position = finalCameraPosition;
        transform.LookAt(lookTarget);

        Vector3 lookDirection = transform.forward;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            target.forward = lookDirection.normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Vector3 lookTarget = target.position + Vector3.up * lookAtHeight + Quaternion.Euler(pitch, yaw, 0f) * lookOffset;
        Vector3 desiredPos = target.position + Vector3.up * lookAtHeight + Quaternion.Euler(pitch, yaw, 0f) * thirdPersonOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lookTarget, desiredPos);
        Gizmos.DrawWireSphere(desiredPos, sphereRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookTarget, 0.1f);
    }
}

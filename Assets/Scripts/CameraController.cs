using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 thirdPersonOffset = new Vector3(0.5f, 0.5f, -1f);
    public Vector3 firstPersonOffset = new Vector3(0f, 0.3f, 0.1f);
    public Vector3 lookOffset = new Vector3(0.5f, 0.5f, 0f);
    public float lookAtHeight = 0.5f;

    [Header("Smoothness & Collision")]
    public float smoothSpeed = 0.2f;
    public float sphereRadius = 0.3f;
    public float minDistance = 0.2f;
    public float firstPersonThreshold = 0.8f;
    public float thirdPersonRestoreThreshold = 1.5f;
    public LayerMask obstacleMask;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 70f;

    private bool inFirstPerson = false;
    private float yaw = 0f;
    private float pitch = 15f;

    private Vector3 targetPosition;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Mausrotation erfassen
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Zielposition vorbereiten
        targetPosition = target.position;
        targetPosition.y += lookAtHeight;

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredThirdPerson = targetPosition + cameraRotation * thirdPersonOffset;
        Vector3 lookTargetThirdPerson = targetPosition + cameraRotation * lookOffset;

        Vector3 direction = desiredThirdPerson - lookTargetThirdPerson;
        float maxDistance = direction.magnitude;

        Vector3 cameraPosition = desiredThirdPerson;
        Vector3 lookTarget = lookTargetThirdPerson;

        bool obstacleDetected = false;
        float actualDistance = maxDistance;

        if (direction.magnitude > 0.01f && Physics.SphereCast(lookTargetThirdPerson, sphereRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleMask))
        {
            obstacleDetected = true;
            actualDistance = hit.distance;
        }

        // Perspektivwechsel-Logik
        if (inFirstPerson)
        {
            if (!obstacleDetected || actualDistance >= thirdPersonRestoreThreshold)
            {
                inFirstPerson = false;
                cameraPosition = desiredThirdPerson;
            }
            else
            {
                cameraPosition = target.position + target.rotation * firstPersonOffset;
                lookTarget = cameraPosition + target.forward * 10f + Vector3.up * 0.5f;
            }
        }
        else
        {
            if (obstacleDetected && actualDistance < firstPersonThreshold)
            {
                inFirstPerson = true;
                cameraPosition = target.position + target.rotation * firstPersonOffset;
                lookTarget = cameraPosition + target.forward * 10f + Vector3.up * 0.5f;
            }
            else if (obstacleDetected)
            {
                float safeDistance = Mathf.Max(minDistance, actualDistance - 0.05f);
                cameraPosition = lookTargetThirdPerson + direction.normalized * safeDistance;
            }
        }

        // Kamera bewegen und ausrichten
        transform.position = Vector3.Lerp(transform.position, cameraPosition, smoothSpeed);
        transform.LookAt(lookTarget);

        // Charakter horizontal zur Kamera ausrichten
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
        Vector3 lookTarget = targetPosition + Quaternion.Euler(pitch, yaw, 0f) * lookOffset;
        Vector3 desiredPos = targetPosition + Quaternion.Euler(pitch, yaw, 0f) * thirdPersonOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lookTarget, desiredPos);
        Gizmos.DrawWireSphere(desiredPos, sphereRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookTarget, 0.1f);
    }
}

using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform target;
    Vector3 targetPosition;
    public Vector3 thirdPersonOffset = new Vector3(0.5f, 0.5f, -1f);
    public Vector3 firstPersonOffset = new Vector3(0f, 0.3f, 0.1f);
    public Vector3 lookOffset = new Vector3(0.5f, 0.5f, 0f);
    public float lookAtHeight = 0.5f;

    public float smoothSpeed = 0.2f;
    public float sphereRadius = 0.3f;
    public float minDistance = 0.2f;
    public float firstPersonThreshold = 0.8f;
    public LayerMask obstacleMask;

    private bool inFirstPerson = false;
    public float thirdPersonRestoreThreshold = 1.5f; // z. B. größer als firstPersonThreshold


    void LateUpdate()
    {
        targetPosition = target.position;
        targetPosition.y = target.position.y + lookAtHeight;
        
        Vector3 desiredThirdPerson = targetPosition + target.rotation * thirdPersonOffset;
        Vector3 lookTargetThirdPerson = targetPosition + target.rotation * lookOffset;

        Vector3 direction = desiredThirdPerson - lookTargetThirdPerson;
        float maxDistance = direction.magnitude;

        Vector3 cameraPosition = desiredThirdPerson;
        Vector3 lookTarget = lookTargetThirdPerson;

        bool obstacleDetected = false;
        float actualDistance = maxDistance;

        // SphereCast nur, wenn Richtung gültig
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
                // Genug Platz → zurück zu Third-Person
                inFirstPerson = false;
                cameraPosition = desiredThirdPerson;
            }
            else
            {
                // Bleib in First-Person
                cameraPosition = targetPosition + target.rotation * firstPersonOffset;
                lookTarget = cameraPosition + target.forward * 10f + Vector3.up * 0.5f;
            }
        }
        else
        {
            if (obstacleDetected && actualDistance < firstPersonThreshold)
            {
                // Zu wenig Platz → in First-Person wechseln
                inFirstPerson = true;
                cameraPosition = targetPosition + target.rotation * firstPersonOffset;
                lookTarget = cameraPosition + target.forward * 10f + Vector3.up * 0.5f;
            }
            else if (obstacleDetected)
            {
                // Kamera näher an Wand positionieren
                float safeDistance = Mathf.Max(minDistance, actualDistance - 0.05f);
                cameraPosition = lookTargetThirdPerson + direction.normalized * safeDistance;
            }
        }

        // Kamera bewegen & ausrichten
        transform.position = Vector3.Lerp(transform.position, cameraPosition, smoothSpeed);
        transform.LookAt(lookTarget);
    }

    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Vector3 lookTarget = targetPosition + target.rotation * lookOffset;
        Vector3 desiredPos = targetPosition + target.rotation * thirdPersonOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lookTarget, desiredPos);
        Gizmos.DrawWireSphere(desiredPos, sphereRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookTarget, 0.1f);
    }
}

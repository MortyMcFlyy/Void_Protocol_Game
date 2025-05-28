using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.zero; 
    public float distance = 5.0f; 
    public float height = 2.0f; 
    public float rotationSpeed = 5.0f; 
    public float zoomSpeed = 2.0f;
    public float minDistance = 3.0f;
    public float maxDistance = 10.0f;

    private float currentDistance;
    private float currentHeight;

    void Start()
    {
        currentDistance = distance;
        currentHeight = height;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Target nicht ausgewählt.");
            return;
        }

/*
        if (Input.GetMouseButton(0))
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed;
            transform.RotateAround(target.position, Vector3.up, horizontalRotation);
        }
*/
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minDistance, maxDistance);

        Vector3 diff = target.position - transform.position;
        Vector3 camPos = target.position - diff.normalized * currentDistance;
        camPos.y = target.position.y + currentHeight;
        transform.position = camPos;
        transform.LookAt(target.position + offset);
    }
}
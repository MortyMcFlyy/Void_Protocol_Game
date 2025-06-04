using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;       // Der Charakter, dem die Kamera folgt
    public Vector3 offset = new Vector3(0, 2, -5); // Höhe und Abstand hinter dem Charakter
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // Zielposition für die Kamera berechnen
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Sanft zur Zielposition bewegen
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Kamera auf den Charakter ausrichten
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}

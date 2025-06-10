using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class GrapplingHook : MonoBehaviour
{
    public Transform grappleOrigin; // z. B. rechte Hand oder Waffe
    public LineRenderer lineRenderer;

    public float maxGrappleDistance = 30f;
    public float grapplePullSpeed = 10f;
    public float grappleShootSpeed = 40f;
    public LayerMask grappleLayer;
    public KeyCode grappleKey = KeyCode.F;

    private Rigidbody rb;
    private Vector3 grapplePoint;
    private bool isGrappling = false;
    private bool isShooting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            Debug.Log("F gedrückt!");
            TryStartGrapple();
        }

        if (Input.GetKeyUp(grappleKey))
        {
            StopGrapple();
        }

        if (isGrappling || isShooting)
        {
            lineRenderer.SetPosition(0, grappleOrigin.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void FixedUpdate()
    {
        if (isGrappling)
        {
            Vector3 dir = (grapplePoint - transform.position).normalized;
            rb.AddForce(dir * grapplePullSpeed, ForceMode.Acceleration);

            if (Vector3.Distance(transform.position, grapplePoint) < 2f)
            {
                StopGrapple();
            }
        }
    }

    void TryStartGrapple()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            Debug.Log("Ziel gefunden: " + hit.collider.name);
            grapplePoint = hit.point;
            StartCoroutine(ShootGrapple());
        }
        else
        {
            Debug.Log("Kein Ziel gefunden.");
        }
    }


    IEnumerator ShootGrapple()
    {
        isShooting = true;
        lineRenderer.enabled = true;

        float t = 0f;
        Vector3 currentPoint = grappleOrigin.position;

        while (Vector3.Distance(currentPoint, grapplePoint) > 0.5f)
        {
            t += Time.deltaTime * grappleShootSpeed;
            currentPoint = Vector3.Lerp(grappleOrigin.position, grapplePoint, t);
            lineRenderer.SetPosition(0, grappleOrigin.position);
            lineRenderer.SetPosition(1, currentPoint);
            yield return null;
        }

        // Nach dem "Schuss" → Grapple aktivieren
        isShooting = false;
        isGrappling = true;
    }

    void StopGrapple()
    {
        isGrappling = false;
        isShooting = false;
        lineRenderer.enabled = false;
    }
}

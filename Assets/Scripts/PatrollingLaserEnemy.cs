using UnityEngine;
using System.Collections;

public class PatrollingLaserEnemy : MonoBehaviour
{
    [Header("Patrouille")]
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("Spieler-Erkennung")]
    public float killRadius = 2f;
    public LayerMask playerLayer;
    public float aggroDelay = 2f;     // Zeit bis Attack startet (Aggro)
    public float shootDelay = 0.5f;    // Zeit nach dem Aggro bis zum Schuss (kann kurz sein)
    public float rotationSpeed = 2f;   // Geschwindigkeit, mit der sich der Gegner zum Spieler dreht

    [Header("Laser / Schuss")]
    public Transform weaponMuzzle;    // muzzle / Waffe-Arm Transform (vom Laserarm)
    public LineRenderer lineRenderer; // LineRenderer für den Laserstrahl
    public float beamDuration = 0.15f;
    public float maxShootDistance = 50f;

    [Header("Animation")]
    public Animator animator;

    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool playerDetected = false;
    private Transform playerTarget;

    void Start()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    void Update()
    {
        if (playerDetected)
        {
            if (playerTarget != null)
                RotateTowards(playerTarget.position);
            return; // Bewegung stoppen, wenn Angriff läuft
        }

        Patrol();
        DetectPlayer();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (waiting) return;

        Transform targetPoint = patrolPoints[currentPointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        // Gegner ausrichten
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.forward = direction;

        // Abstand zum Zielpunkt
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // Animator: speed hoch wenn noch nicht am Ziel, sonst 0
        float animSpeed = distance > 0.05f ? moveSpeed : 0f;
        if (animator != null)
            animator.SetFloat("speed", animSpeed);

        // Am Wegpunkt angekommen?
        if (distance < 0.05f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    private IEnumerator WaitAtPoint()
    {
        waiting = true;

        if (animator != null)
            animator.SetFloat("speed", 0f); // Idle erzwingen
        yield return new WaitForSeconds(waitTimeAtPoint);

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        waiting = false;
    }

    private void DetectPlayer()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, killRadius, playerLayer);
        if (hitPlayers.Length > 0)
        {
            playerDetected = true;
            playerTarget = hitPlayers[0].transform;
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        if (playerTarget == null)
        {
            playerDetected = false;
            yield break;
        }

        // Spieler-Controller und Rigidbody
        var pc = playerTarget.GetComponent<PlayerController>();
        var rb = playerTarget.GetComponent<Rigidbody>();
        var playerAnimator = playerTarget.GetComponent<Animator>();

        if (pc.IsDead())
        {
            playerDetected = false;
            yield break;
        }

        // Spielerbewegung sofort blockieren (wie im Original)
        if (pc != null) pc.canMove = false;
        if (rb != null)
        {
            // stoppe sofortige Bewegung
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerAnimator != null)
        {
            // Erzwinge Idle beim Spieler
            playerAnimator.SetFloat("Speed", 0f);
        }

        // Aggro-Animation des Gegners starten (z.B. hebt Arm oder zielt)
        if (animator != null)
            animator.SetTrigger("Aggro");

        // Warte kurz bis Aggro-Phase vorbei ist (zielen)
        yield return new WaitForSeconds(aggroDelay);        
            
        // kurze Verzögerung bevor der Kill ausgeführt wird
        yield return new WaitForSeconds(shootDelay);

        // Laser abschießen (zeichnet LineRenderer)
        yield return StartCoroutine(FireLaser());

        // Spieler sterben lassen (ruft die gleiche Methode wie im ursprünglichen Skript auf)
        if (pc != null)
        {
            pc.Die();
        }
        else
        {
            // Falls kein PlayerController vorhanden, fallback: zerstöre GameObject
            Destroy(playerTarget.gameObject);
        }

        if (animator != null)
            animator.SetTrigger("Shoot");

        playerDetected = false;
    }

    private IEnumerator FireLaser()
    {
        if (lineRenderer == null || weaponMuzzle == null || playerTarget == null)
            yield break;

        Vector3 start = weaponMuzzle.position;
        Vector3 targetPos = playerTarget.position;
        // Versetze Ziel leicht nach oben, damit der Strahl auf rote target zone (Brust/Kopf) trifft
        targetPos += Vector3.up * 0.5f;

        // Raycast: ermittle exaktes Trefferpunkt (z. B. Hindernisse dazwischen)
        Vector3 dir = (targetPos - start).normalized;
        RaycastHit hit;
        Vector3 end = start + dir * maxShootDistance;
        if (Physics.Raycast(start, dir, out hit, maxShootDistance))
        {
            end = hit.point;
        }
        else
        {
            // wenn nichts getroffen, dann ziehe Linie zum Spieler (oder maximal)
            float distToPlayer = Vector3.Distance(start, targetPos);
            end = start + dir * Mathf.Min(distToPlayer, maxShootDistance);
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        // kurze Flackerdauer des Lasers
        float elapsed = 0f;
        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            // Optional: animiere Strength/Intensity über Material (wenn Material-Property vorhanden)
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    // Glattes Drehen zur Spielerposition (nur Y-Achse, kein Kippen)
    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f; // nur um die Y-Achse drehen
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    // Visualisierung des Kill-Radius im Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red * 0.6f;
        Gizmos.DrawWireSphere(transform.position, killRadius);
        if (weaponMuzzle != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(weaponMuzzle.position, 0.05f);
        }
    }
}
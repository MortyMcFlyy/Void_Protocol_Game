using UnityEngine;
using System.Collections;

public class PatrollingEnemy : MonoBehaviour
{
    [Header("Patrouille")]
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("Spieler-Erkennung")]
    public float killRadius = 2f;
    public LayerMask playerLayer;
    public float aggroDelay = 1f;     // Zeit bis Attack startet
    public float killDelay = 1f;      // Zeit bis Kill nach Aggro

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private Transform grabPoint; // im Inspector zuweisen

    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool playerDetected = false;
    private Transform playerTarget;

    void Update()
    {
        if (playerDetected) return; // Bewegung stoppen, wenn im Kampf

        Patrol();
        DetectPlayer();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;
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

        animator.SetFloat("speed", 0f); // sicherstellen dass Idle läuft
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
        // Spieler-Controller und Rigidbody
        var pc = playerTarget.GetComponent<PlayerController>();
        var rb = playerTarget.GetComponent<Rigidbody>();
        var playerAnimator = playerTarget.GetComponent<Animator>();

        // Spielerbewegung sofort blockieren
        if (pc != null) pc.canMove = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Spieler-Animator Walk stoppen
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f); // Idle erzwingen
        }

        // Aggro-Animation des Gegners starten
        animator.SetTrigger("Aggro");

        // Aggro-Phase: Spieler langsam zum Gegner ziehen
        float elapsed = 0f;
        Vector3 startPos = playerTarget.position;
        Quaternion startRot = playerTarget.rotation;

        // Bestimme den Punkt, wo der Spieler in den Händen des Gegners landen soll
        Vector3 targetPos = transform.position;
        //grabPoint.rotation = grabPoint.rotation * Quaternion.Euler(90f, 0, 0); // 90° Rotation

        if (rb != null)
        {
            rb.isKinematic = true;        // Physik deaktivieren
            rb.detectCollisions = false;
            rb.freezeRotation = false;
        }

        while (elapsed < aggroDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / aggroDelay;

            // Spieler langsam Richtung Gegner bewegen
            playerTarget.position = Vector3.Lerp(startPos, targetPos, t);

            // Spieler dabei nach und nach zum Gegner ausrichten
            Vector3 lookDir = (transform.position - playerTarget.position).normalized;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                playerTarget.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(lookDir), t);
            }

            yield return null;
        }

        // Endposition setzen und Physik deaktivieren
        playerTarget.position = grabPoint.position;
        playerTarget.rotation = grabPoint.rotation;
        playerTarget.SetParent(grabPoint);

        

        // Kill-Animation starten
        animator.SetTrigger("Kill");
        yield return new WaitForSeconds(killDelay);

        // Spieler sterben lassen
        if (pc != null)
        {
            pc.Die(); // Teleportiert ihn zum Spawn oder macht was sonst vorgesehen ist
        }

        if (rb != null)
        {
            rb.isKinematic = false; // Physik wieder aktivieren
            rb.detectCollisions = true;
            rb.freezeRotation = true;
        }
        playerTarget.SetParent(null);
        playerDetected = false;
    }

}

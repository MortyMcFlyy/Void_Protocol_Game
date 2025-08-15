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
    public float attackDelay = 1f;    // Zeit bis Kill nach Attack

    [Header("Animation")]
    public Animator animator;

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

        if (waiting)
            return;

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

        // Walk-Animation starten
        animator.SetTrigger("Walk");

        // Am Wegpunkt angekommen?
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    private IEnumerator WaitAtPoint()
    {
        waiting = true;

        // Idle-Animation abspielen
        animator.SetTrigger("Idle");
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
        // Bewegung des Spielers sofort blockieren
        var pc = playerTarget.GetComponent<PlayerController>();
        if (pc != null) pc.canMove = false;

        var rb = playerTarget.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Aggro-Animation
        animator.SetTrigger("Aggro");
        yield return new WaitForSeconds(aggroDelay);

        // Zufällige Attacke
        if (Random.value > 0.5f)
            animator.SetTrigger("AttackSmall");
        else
            animator.SetTrigger("AttackBig");

        yield return new WaitForSeconds(attackDelay);

        // Kill-Animation
        animator.SetTrigger("Kill");
        yield return new WaitForSeconds(1f);

        // Spieler sterben lassen
        if (pc != null)
        {
            pc.Die(); // In Die() teleportierst du ihn zum Spawn
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}

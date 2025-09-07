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
    public float aggroDelay = 1f;
    public float killDelay = 1f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource walkSound;

    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool playerDetected = false;
    private Transform playerTarget;

    void Update()
    {
        if (playerDetected) return;

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

        Vector3 direction = (targetPoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.forward = direction;

        float distance = Vector3.Distance(transform.position, targetPoint.position);

        float animSpeed = distance > 0.05f ? moveSpeed : 0f;
        animator.SetFloat("speed", animSpeed);

        if (!walkSound.isPlaying)
        {
            walkSound.Play();
        }

        if (distance < 0.05f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }



    private IEnumerator WaitAtPoint()
    {
        waiting = true;
        walkSound.Stop();

        animator.SetFloat("speed", 0f);
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
        var pc = playerTarget.GetComponent<PlayerController>();
        var rb = playerTarget.GetComponent<Rigidbody>();
        var playerAnimator = playerTarget.GetComponent<Animator>();

        if (pc != null) pc.canMove = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f); 
        }

        animator.SetTrigger("Aggro");
        walkSound.Stop();
        attackSound?.Play();

        float elapsed = 0f;
        Vector3 startPos = playerTarget.position;
        Quaternion startRot = playerTarget.rotation;

        Vector3 targetPos = grabPoint.position;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.freezeRotation = false;
        }

        yield return new WaitForSeconds(2.5f);

        while (elapsed < aggroDelay/2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (aggroDelay/2f);

            playerTarget.position = Vector3.Lerp(startPos, targetPos, t);

            Vector3 lookDir = (transform.position - playerTarget.position).normalized;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                playerTarget.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(lookDir), t);
            }

            yield return null;
        }

        animator.SetTrigger("Kill");
        yield return new WaitForSeconds(killDelay);

        if (pc != null)
        {
            pc.Die(PlayerController.DeathType.Laser);
        }

        
        playerTarget.SetParent(null);
        playerDetected = false;
    }

}

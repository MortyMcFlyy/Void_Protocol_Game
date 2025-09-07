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
    public float aggroDelay = 2f;
    public float shootDelay = 0.5f;
    public float rotationSpeed = 2f;

    [Header("Laser / Schuss")]
    public Transform weaponMuzzle;
    public LineRenderer lineRenderer;
    public float beamDuration = 0.15f;
    public float maxShootDistance = 50f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource walkSound;

    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool playerDetected = false;
    private Transform playerTarget;
    private bool isBaseLineMode = false;

    void Start()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isBaseLineMode) return;
        if (playerDetected)
        {
            if (playerTarget != null)
                RotateTowards(playerTarget.position);
            return;
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

        Vector3 direction = (targetPoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.forward = direction;

        float distance = Vector3.Distance(transform.position, targetPoint.position);

        float animSpeed = distance > 0.05f ? moveSpeed : 0f;
        if (animator != null)
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

        if (animator != null)
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
        if (playerTarget == null)
        {
            playerDetected = false;
            yield break;
        }

        var pc = playerTarget.GetComponent<PlayerController>();
        var rb = playerTarget.GetComponent<Rigidbody>();
        var playerAnimator = playerTarget.GetComponent<Animator>();

        if (pc.IsDead())
        {
            playerDetected = false;
            yield break;
        }

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

        walkSound.Stop();

        if (animator != null)
            animator.SetTrigger("Aggro");

        yield return new WaitForSeconds(aggroDelay);        
            
        yield return new WaitForSeconds(shootDelay);

        yield return StartCoroutine(FireLaser());

        if (pc != null)
        {
            pc.Die(PlayerController.DeathType.Laser);
        }
        else
        {
            Destroy(playerTarget.gameObject);
        }

        if (animator != null)
            animator.SetTrigger("Shoot");

        playerDetected = false;
    }

    private IEnumerator FireLaser()
    {
        attackSound?.Play();

        if (lineRenderer == null || weaponMuzzle == null || playerTarget == null)
            yield break;

        Vector3 start = weaponMuzzle.position;
        Vector3 targetPos = playerTarget.position;
        targetPos += Vector3.up * 0.5f;

        Vector3 dir = (targetPos - start).normalized;
        RaycastHit hit;
        Vector3 end = start + dir * maxShootDistance;
        if (Physics.Raycast(start, dir, out hit, maxShootDistance))
        {
            end = hit.point;
        }
        else
        {
            float distToPlayer = Vector3.Distance(start, targetPos);
            end = start + dir * Mathf.Min(distToPlayer, maxShootDistance);
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        float elapsed = 0f;
        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

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

    public void SetBaseLineMode(bool isActive)
    {
        isBaseLineMode = isActive;
        if (isBaseLineMode)
        {
            playerDetected = false;
            StopAllCoroutines();
            if (animator != null)
                animator.SetFloat("speed", 0f);
            walkSound.Stop();
        }
    }
}
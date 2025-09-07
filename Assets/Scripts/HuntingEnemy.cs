using UnityEngine;
using System.Collections;

public class HuntingEnemy : MonoBehaviour
{
    [Header("Verfolgung")]
    public float moveSpeed = 3f;
    public float killRadius = 2f;
    public LayerMask playerLayer;
    public float aggroDelay = 1f;
    public float killDelay = 1f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource walkSound;

    private Transform playerTarget;
    private bool playerDetected = false;
    private bool huntStarted = false;

    void Update()
    {
        if (!huntStarted) return;
        if (playerDetected) return;

        FindAndFollowPlayer();
        DetectPlayer();
    }

    private void FindAndFollowPlayer()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 30f, playerLayer); // 30f = Suchradius
        if (hitPlayers.Length > 0)
        {
            playerTarget = hitPlayers[0].transform;
            MoveTowardsPlayer();
        }
        else
        {
            animator.SetFloat("speed", 0f);
            walkSound.Stop();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (playerTarget == null) return;

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
            transform.forward = direction;

        animator.SetFloat("speed", moveSpeed);

        if (!walkSound.isPlaying)
        {
            walkSound.Play();
        }
    }

    private void DetectPlayer()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= killRadius)
        {
            playerDetected = true;
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

    public void StartHunt()
    {
        huntStarted = true;
        Debug.Log("Roboter-Jagd gestartet!");
    }
}

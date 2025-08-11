using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrouille")]
    public Transform[] waypoints;
    public float waitTime = 1f;
    private int currentIndex = 0;
    private float waitTimer;

    [Header("Spielererkennung")]
    public Transform player;
    public float detectionRange = 5f;
    public float attackRange = 2f;

    [Header("Komponenten")]
    private Animator animator;
    private NavMeshAgent agent;

    private enum EnemyState { Idle, Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (distToPlayer <= detectionRange)
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                agent.SetDestination(player.position);
                animator.Play("Walk");
                if (distToPlayer <= attackRange)
                    ChangeState(EnemyState.Attack);
                else if (distToPlayer > detectionRange + 2f)
                    ChangeState(EnemyState.Patrol);
                break;

            case EnemyState.Attack:
                agent.ResetPath();
                transform.LookAt(player);
                if (distToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                else
                {
                    // Animation zufällig auswählen
                    if (Random.value > 0.5f)
                        animator.Play("Attack Small");
                    else
                        animator.Play("Attack Big");

                    // Spieler „töten“
                    KillPlayer();
                }
                break;

            case EnemyState.Idle:
                animator.Play("Idle");
                break;
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentIndex].position);
                animator.Play("Walk");
                waitTimer = 0f;
            }
            else
            {
                animator.Play("Idle Transition");
            }
        }
    }

    void KillPlayer()
    {
        Debug.Log("Spieler getötet!");
        animator.Play("Kill");
        // Respawn oder Game Over kannst du hier antriggern
    }

    void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float mouseSensitivity = 250f;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GrapplingHook grapple;

    [Header("Respawn Settings")]
    public Transform spawnPoint;         
    public float respawnDelay = 1f;      
    public LayerMask killZone;           
    public bool canMove = true;

    [Header("Fade Settings")]
    public CanvasGroup fadePanel;        
    public float fadeDuration = 1f;
    public bool isTeleporting = false;      

    private bool isDead = false;         
    private Rigidbody rb;
    private bool jumpRequested = false;
    private Vector3 movementInput = Vector3.zero;
    private Vector3 conveyorVelocity = Vector3.zero;


    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        groundCheck.localPosition = new Vector3(0, -0.9f, 0); // Setze die Position des GroundChecks relativ zum Spieler
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
        }
    }

    void Update()
    {
        if (!canMove) return;

        // Bewegungseingaben erfassen
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        movementInput = (transform.forward * moveZ + transform.right * moveX).normalized * moveSpeed;

        // Sprunganforderung setzen
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpRequested = true;
        }

        //Animationsstatus aktualisieren

        animator.SetBool("isGrounded", IsGrounded());

        float moveMagnitude = new Vector2(moveX, moveZ).magnitude;
        animator.SetFloat("Speed", moveMagnitude);

        // Sprung starten
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpRequested = true;
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
        }
        
        // Debugging
        //Debug.DrawRay(groundCheck.position, Vector3.down * 0.2f, Color.red);
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 currentVel = rb.linearVelocity;

        if (!grapple.IsGrappling())
        {
            Vector3 desiredXZ = movementInput + conveyorVelocity;
            currentVel.x = desiredXZ.x;
            currentVel.z = desiredXZ.z;
            rb.linearVelocity = currentVel;

        }


        // Springen
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }


    bool IsGrounded()
    {
        bool grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded)
        {
            animator.SetBool("isGrounded", true);
        }
        else
        {
            animator.SetBool("isGrounded", false);
        }
        return grounded;
    }

    public void SetConveyorVelocity(Vector3 velocity)
    {
        conveyorVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & killZone) != 0 && !isDead)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & killZone) != 0 && !isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 Player ist gestorben!");

        // Coroutine statt Invoke, damit wir auch den Fade einbauen können
        StartCoroutine(FadeAndRespawn());
    }

    public bool IsDead()
    {
        return isDead;
    }

    private IEnumerator FadeAndRespawn()
    {
        // Bildschirm zu schwarz
        yield return StartCoroutine(Fade(0f, 1f));

        // Warten während Schwarz
        yield return new WaitForSeconds(respawnDelay);

        // Respawn durchführen
        Respawn();

        // Schwarz wieder ausblenden
        yield return StartCoroutine(Fade(1f, 0f));

        isDead = false;
    }

    private void Respawn()
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // Falls der Spieler einen Rigidbody hat, Bewegungen zurücksetzen
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.freezeRotation = true;
            
        }
        canMove = true;


        Debug.Log("🔄 Player respawned!");
    }

    public IEnumerator Fade(float start, float end)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = end;
    }

    public void SetSpawnPoint(Transform t)
    {
        if (t == null) return;
        spawnPoint = t;
    }

}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float mouseSensitivity = 250f;
    private float rotationY = 0f;

    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool jumpRequested = false;
    private Vector3 movementInput = Vector3.zero;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        groundCheck.localPosition = new Vector3(0, -0.9f, 0); // Setze die Position des GroundChecks relativ zum Spieler
    }

    void Update()
    {
        // Mausrotation (nicht physikbasiert → bleibt in Update)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

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
        // Bewegung anwenden
        Vector3 velocity = rb.linearVelocity;
        velocity.x = movementInput.x;
        velocity.z = movementInput.z;
        rb.linearVelocity = velocity;

        // Springen ausführen
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

}
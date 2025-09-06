using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public enum DeathType
    {
        Acid,  // Killzone/Säure
        Laser  // Laser
    }

    [Header("Movement Settings")]
    public float mouseSensitivity = 250f;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GrapplingHook grapple;
    public KeyCode lightKey = KeyCode.E;
    public Light playerLight;
    public AudioSource walkSound;


    [Header("Respawn Settings")]
    public Transform spawnPoint;         
    public float respawnDelay = 1f;      
    public LayerMask killZone;           
    public bool canMove = true;

    [Header("Fade Settings")]
    public CanvasGroup fadePanel;        
    public float fadeDuration = 1f;
    public bool isTeleporting = false;      

    [Header("Dissolve Effect")]
    public Material dissolveMaterial;  // Hier "Mat_dissolve_Green" zuweisen
    public Material laserDissolveMaterial;  // Hier "Mat_dissolve_Red" zuweisen
    public Transform robotModel;       // ReRoboHookIdle Transform zuweisen
    public float dissolveTime = 2.0f;  // Zeit für den Dissolve-Effekt
    public DeathType currentDeathType;  // Speichert die aktuelle Todesursache

    // Speichere die Renderer und ihre ursprünglichen Materialien
    private Renderer[] characterParts;
    private Material[][] originalMaterials;

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
        
        // Finde alle Renderer der Charakterteile und speichere ihre Materialien
        SetupDissolveComponents();
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

        // Licht ein-/ausschalten
        if (Input.GetKeyDown(lightKey) && playerLight != null)
        {
            playerLight.enabled = !playerLight.enabled;
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
        if (currentVel.magnitude > 0.1f && IsGrounded())
        {
            if (!walkSound.isPlaying)
            {
                walkSound.Play();
            }
        }
        else
        {
            walkSound.Stop();
        }
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
            collision.gameObject.GetComponent<AudioSource>()?.Play();
            Die(DeathType.Acid); // Explizit als Säure-Tod markieren
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & killZone) != 0 && !isDead)
        {
            other.gameObject.GetComponent<AudioSource>()?.Play();
            Die(DeathType.Laser); // Explizit als Säure-Tod markieren
        }
    }

    // Füge den DeathType-Parameter hinzu
    public void Die(DeathType deathType = DeathType.Acid)
    {
        if (isDead) return;
        isDead = true;
        currentDeathType = deathType;  // Speichere die Todesursache
        
        Debug.Log($"💀 Player ist gestorben! Ursache: {deathType}");

        // Dissolve-Effekt starten
        StartCoroutine(PlayDissolveEffect());
        
        // Coroutine statt Invoke, damit wir auch den Fade einbauen können
        StartCoroutine(FadeAndRespawn());
    }
    
    public bool IsDead()
    {
        return isDead;
    }

    public IEnumerator PlayDissolveEffect()
    {
        if (characterParts == null) yield break;

        // Wähle das Material basierend auf der Todesursache
        Material effectMaterial = currentDeathType == DeathType.Laser && laserDissolveMaterial != null 
                                 ? laserDissolveMaterial 
                                 : dissolveMaterial;
        
        if (effectMaterial == null) yield break;

        // Erstelle Instanz des ausgewählten Dissolve-Materials
        Material instancedDissolveMaterial = new Material(effectMaterial);

        // Setze den Dissolve-Faktor auf 0
        instancedDissolveMaterial.SetFloat("_DissolveFactor", 0f);

        // Wende das Material auf alle Teile an
        for (int i = 0; i < characterParts.Length; i++)
        {
            if (characterParts[i] != null)
            {
                Material[] newMaterials = new Material[characterParts[i].materials.Length];
                for (int j = 0; j < newMaterials.Length; j++)
                {
                    newMaterials[j] = instancedDissolveMaterial;
                }
                characterParts[i].materials = newMaterials;
            }
        }

        // Animiere den Dissolve-Faktor von 0 zu 1
        float elapsed = 0f;
        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;
            float dissolveFactor = Mathf.Lerp(0f, 1f, elapsed / dissolveTime);
            instancedDissolveMaterial.SetFloat("_DissolveFactor", dissolveFactor);
            yield return null;
        }
    }

    private IEnumerator FadeAndRespawn()
    {
        // Warte bis der Dissolve-Effekt teilweise abgeschlossen ist
        yield return new WaitForSeconds(dissolveTime * 0.5f);
        
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
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.freezeRotation = true;
        }
        canMove = true;

        // Setze originale Materialien zurück
        RestoreOriginalMaterials();

        Debug.Log("🔄 Player respawned!");
    }

    private void RestoreOriginalMaterials()
    {
        if (characterParts == null || originalMaterials == null) return;
        
        for (int i = 0; i < characterParts.Length; i++)
        {
            if (characterParts[i] != null && originalMaterials[i] != null)
            {
                Material[] currentMaterials = new Material[originalMaterials[i].Length];
                for (int j = 0; j < originalMaterials[i].Length; j++)
                {
                    currentMaterials[j] = originalMaterials[i][j];
                }
                characterParts[i].materials = currentMaterials;
            }
        }
    }

    void SetupDissolveComponents()
    {
        if (robotModel == null)
        {
            Debug.LogWarning("Robot model reference is missing!");
            return;
        }

        // Finde alle Renderer in den gewünschten Teilen
        string[] partNames = { "arms.001", "body.001", "hips.001", "lowerLegs.001", "upperLegs.001","eyeBeeg.001","eyeSmol.001" };
        characterParts = new Renderer[partNames.Length];
        originalMaterials = new Material[partNames.Length][];
        
        for (int i = 0; i < partNames.Length; i++)
        {
            Transform part = robotModel.Find(partNames[i]);
            if (part != null)
            {
                Renderer renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    characterParts[i] = renderer;
                    // Sichere die originalen Materialien
                    originalMaterials[i] = new Material[renderer.materials.Length];
                    for (int j = 0; j < renderer.materials.Length; j++)
                    {
                        originalMaterials[i][j] = renderer.materials[j];
                    }
                }
                else
                {
                    Debug.LogWarning($"No renderer found on {partNames[i]}");
                }
            }
            else
            {
                Debug.LogWarning($"Part {partNames[i]} not found");
            }
        }
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
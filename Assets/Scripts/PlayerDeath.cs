using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform spawnPoint;         // Wo der Spieler wieder erscheinen soll
    public float respawnDelay = 1f;      // Zeit bis Respawn
    public LayerMask killZone;           // Layer für tödliche Objekte

    [Header("Fade Settings")]
    public CanvasGroup fadePanel;        // Schwarzes UI-Panel mit CanvasGroup
    public float fadeDuration = 1f;      // Dauer für Fade in/out

    private bool isDead = false;         // Verhindert mehrfaches Auslösen

    private void Start()
    {
        // Sicherstellen, dass der Bildschirm am Anfang sichtbar ist
        if (fadePanel != null)
            fadePanel.alpha = 0f;
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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("🔄 Player respawned!");
    }

    private IEnumerator Fade(float start, float end)
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
}

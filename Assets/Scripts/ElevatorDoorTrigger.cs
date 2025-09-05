using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ElevatorDoorTrigger : MonoBehaviour
{
    [Header("Türen / Wände")]
    [SerializeField] private Door entryDoor;          // Tür/Wand unten (fährt hoch)
    [SerializeField] private Door exitDoor;           // Tür/Wand oben (fährt hoch nach Teleport) – optional
    [SerializeField] private AudioSource doorAudio;

    [Header("Ziel")]
    [SerializeField] private Transform destinationPoint;  // Position & Rotation wohin der Player teleportiert

    [Header("Fade & Zeiten")]
    [Tooltip("Zusätzliche Wartezeit nach Tür-zu (wenn du z.B. Sound ausklingen lassen willst)")]
    [SerializeField] private float delayAfterEntryDoorOpen = 0f;
    [Tooltip("Wie lange der Bildschirm komplett schwarz bleibt bevor wieder aufgehellt wird.")]
    [SerializeField] private float blackHoldTime = 0.1f;
    [Tooltip("Wartezeit nach Teleport bevor die obere Tür öffnet.")]
    [SerializeField] private float delayBeforeExitDoorOpens = 0.15f;

    [Header("Optionen")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private string playerTag = "Player";

    private bool busy = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (busy) return;
        if (oneShot && busy) return;

        var playerController = other.GetComponent<PlayerController>();
        if (!playerController)
        {
            Debug.LogWarning("ElevatorDoorTrigger: PlayerController nicht gefunden auf Player.");
            return;
        }

        if (!destinationPoint)
        {
            Debug.LogError("ElevatorDoorTrigger: destinationPoint nicht gesetzt!");
            return;
        }

        StartCoroutine(RunSequence(playerController));
    }

    private IEnumerator RunSequence(PlayerController player)
    {
        busy = true;

        // Spieler Input aus
        player.GetComponent<Animator>().SetFloat("Speed", 0f);
        player.canMove = false;

        // 1. Untere Tür öffnen (hochfahren)
        if (entryDoor)
        {
            if (!entryDoor.IsOpen && !entryDoor.IsMoving)
            {
                entryDoor.OpenExternally();
                doorAudio?.Play();
            }

            // Warten bis wirklich offen
            yield return StartCoroutine(WaitUntil(() => entryDoor.IsOpen && !entryDoor.IsMoving));
        }

        if (delayAfterEntryDoorOpen > 0f)
            yield return new WaitForSeconds(delayAfterEntryDoorOpen);

        // 2. Fade zu schwarz
        // ANPASSEN falls deine Fade-Methode anders heißt:
        yield return StartCoroutine(player.Fade(0f, 1f));

        // 3. Kurzer Black Hold optional später (erst Teleport)
        // Physik neutralisieren
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Teleport
        player.transform.SetPositionAndRotation(destinationPoint.position, destinationPoint.rotation);
        player.SetSpawnPoint(destinationPoint);

        if (blackHoldTime > 0f)
            yield return new WaitForSeconds(blackHoldTime);

        // 4. Fade zurück
        // ANPASSEN falls anders:
        yield return StartCoroutine(player.Fade(1f, 0f));

        // 5. Obere Tür öffnen
        if (exitDoor)
        {
            if (delayBeforeExitDoorOpens > 0f)
                yield return new WaitForSeconds(delayBeforeExitDoorOpens);

            if (!exitDoor.IsOpen && !exitDoor.IsMoving)
            {
                exitDoor.OpenExternally();
                doorAudio?.Play();
            }
            // Warten bis offen (nur wenn du willst)
            // yield return StartCoroutine(WaitUntil(() => exitDoor.IsOpen && !exitDoor.IsMoving));
        }

        // 6. Spieler wieder bewegen lassen
        player.canMove = true;

        if (!oneShot)
            busy = false;
    }

    private IEnumerator WaitUntil(System.Func<bool> predicate)
    {
        while (!predicate())
            yield return null;
    }
}
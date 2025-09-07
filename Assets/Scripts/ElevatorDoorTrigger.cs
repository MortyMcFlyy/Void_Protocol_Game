using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ElevatorDoorTrigger : MonoBehaviour
{
    [Header("Türen / Wände")]
    [SerializeField] private Door entryDoor;         
    [SerializeField] private Door exitDoor;        
    [SerializeField] private AudioSource doorAudio;

    [Header("Ziel")]
    [SerializeField] private Transform destinationPoint;

    [Header("Fade & Zeiten")]
    [SerializeField] private float delayAfterEntryDoorOpen = 0f;
    [SerializeField] private float blackHoldTime = 0.1f;
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

        player.GetComponent<Animator>().SetFloat("Speed", 0f);
        player.canMove = false;

        if (entryDoor)
        {
            if (!entryDoor.IsOpen && !entryDoor.IsMoving)
            {
                entryDoor.OpenExternally();
                doorAudio?.Play();
            }

            yield return StartCoroutine(WaitUntil(() => entryDoor.IsOpen && !entryDoor.IsMoving));
        }

        if (delayAfterEntryDoorOpen > 0f)
            yield return new WaitForSeconds(delayAfterEntryDoorOpen);

        yield return StartCoroutine(player.Fade(0f, 1f));

        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(destinationPoint.position, destinationPoint.rotation);
        player.SetSpawnPoint(destinationPoint);

        if (blackHoldTime > 0f)
            yield return new WaitForSeconds(blackHoldTime);

        yield return StartCoroutine(player.Fade(1f, 0f));

        if (exitDoor)
        {
            if (delayBeforeExitDoorOpens > 0f)
                yield return new WaitForSeconds(delayBeforeExitDoorOpens);

            if (!exitDoor.IsOpen && !exitDoor.IsMoving)
            {
                exitDoor.OpenExternally();
                doorAudio?.Play();
            }
        }

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
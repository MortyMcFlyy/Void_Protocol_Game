using UnityEngine;

public class GigaBotAlertTrigger : MonoBehaviour
{
    public AudioSource alertAudio;
    public Light alertLight;
    public float triggerRadius = 2f;
    public LayerMask playerLayer;
    public bool playerDetected = false;

    void Update()
    {
        if (playerDetected) return;
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, triggerRadius, playerLayer);
        if (hitPlayers.Length > 0)
        {
            playerDetected = true;
            alertLight.enabled = true;
            alertAudio?.Play();
            Debug.Log("GigaBot Alarm ausgelöst!");
        }
    }
}

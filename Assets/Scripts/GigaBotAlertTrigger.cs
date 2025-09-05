using UnityEngine;

public class GigaBotAlertTrigger : MonoBehaviour
{
    public AudioSource alertAudio;
    public float triggerRadius = 2f;
    public LayerMask playerLayer;
    public bool playerDetected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDetected) return;
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, triggerRadius, playerLayer);
        if (hitPlayers.Length > 0)
        {
            playerDetected = true;
            alertAudio?.Play();
            Debug.Log("GigaBot Alarm ausgelöst!");
        }
    }
}

using UnityEngine;

public class RobotHuntTrigger : MonoBehaviour
{
    public Door doorToOpen;
    public AudioSource doorSound;
    public AudioSource huntMusic;
    public HuntingEnemy[] robotsToActivate;
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
            doorSound?.Play();
            doorToOpen?.OpenExternally();
            foreach (var robot in robotsToActivate)
            {
                robot?.StartHunt();
            }
            Debug.Log("Roboter-Jagd ausgelöst!");
        }
    }

    public void ResetHunter()
    {
        playerDetected = false;
        foreach (var robot in robotsToActivate)
        {
            robot?.ResetHunter();
        }
    }
}

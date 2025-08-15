using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public Transform spawnPoint; 

    public void Die()
    {
        Debug.Log("Player ist gestorben!");

        // Spieler zurück zum Spawn
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        
    }
}

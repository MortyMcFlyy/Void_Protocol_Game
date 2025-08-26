using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Door door; // Referenz zur Tür

    private void OnTriggerEnter(Collider other)
    {
        // Prüfen, ob der Player den Trigger berührt
        if (other.CompareTag("Player"))
        {
            if (door != null)
            {
                door.OpenExternally(); // Tür-Methode ausführen
            }
        }
    }
}

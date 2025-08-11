using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Vector3 conveyorVelocity = new Vector3(0, 0, -2f); // Richtung & Geschwindigkeit

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Laufband schiebt: " + other.name);
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetConveyorVelocity(conveyorVelocity);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetConveyorVelocity(Vector3.zero);
            }
        }
    }
}


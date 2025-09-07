using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Vector3 conveyorVelocity = new Vector3(-10, 0, 0f);

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

    public void ToggleDirection()
    {
        conveyorVelocity = new Vector3(5, 0, 0);
        Debug.Log("Laufband-Richtung geändert: " + conveyorVelocity);
    }
}


using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName;
    public GrapplingHook playerGrappleScript;

    public string GetPrompt() => $"F: {itemName} aufnehmen";

    public void Interact()
    {
        // Inventar hinzufügen, Objekt entfernen
        Debug.Log($"{itemName} wurde aufgenommen.");
        playerGrappleScript.UnlockGrapple();
        Destroy(gameObject);
    }
}


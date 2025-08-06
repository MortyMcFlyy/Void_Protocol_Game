using UnityEngine;

public interface IInteractable
{
    string GetPrompt();           // z. B. "F: Tür öffnen"
    void Interact();              // Was passiert bei Interaktion
}

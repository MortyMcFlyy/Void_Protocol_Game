using UnityEngine;

public class FuseBoxInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject puzzleUI;

    public string GetPrompt()
    {
        return "F: Sicherungskasten öffnen";
    }

    public void Interact()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
            Time.timeScale = 0f; // Optional: Spiel pausieren
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

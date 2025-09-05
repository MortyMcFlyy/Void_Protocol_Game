using UnityEngine;

public class Laptop : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject passwordUI;   // Canvas mit Eingabefeldern
    [SerializeField] private string correctCode = "1234"; // Richtiger Code
    [SerializeField] private Door linkedDoor;         // Fahrstuhltür, die aufgeht
    [SerializeField] private AudioSource doorAudio;

    private bool isSolved = false;

    private void Start()
    {
        if (passwordUI != null)
            passwordUI.SetActive(false); // UI am Anfang aus
    }

    public string GetPrompt()
    {
        return isSolved ? "" : "F: Laptop benutzen";
    }

    public void Interact()
    {
        if (isSolved) return;

        if (passwordUI != null)
        {
            passwordUI.SetActive(true);
            // Cursor freischalten
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // optional Spiel pausieren
        }
    }

    public void CheckCode(string enteredCode)
    {
        if (enteredCode == correctCode)
        {
            Debug.Log("✅ Passwort korrekt!");
            isSolved = true;

            if (linkedDoor != null)
            {
                linkedDoor.OpenExternally();
                doorAudio?.Play();
            }

            // UI schließen
            CloseUI();
        }
        else
        {
            Debug.Log("❌ Falsches Passwort!");
        }
    }

    public void CloseUI()
    {
        if (passwordUI != null)
            passwordUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f; // wieder normal
    }
}

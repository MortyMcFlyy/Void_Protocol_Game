using UnityEngine;

public class PasswordPanel : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject passwordUI;   
    [SerializeField] private string correctCode = "000"; 
    [SerializeField] private Transform[] energyTower;
    [SerializeField] private AudioSource shutdownAudio;        

    private bool isSolved = false;

    private void Start()
    {
        if (passwordUI != null)
            passwordUI.SetActive(false);
    }

    void Update()
    {
        if (isSolved) return;

        if (Input.GetKeyDown(KeyCode.Escape) && passwordUI.activeSelf)
        {
            CloseUI();
        }
    }

    public string GetPrompt()
    {
        return isSolved ? "" : "F: Passwortfeld öffnen";
    }

    public void Interact()
    {
        if (isSolved) return;

        if (passwordUI != null)
        {
            passwordUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }

    public void CheckCode(string enteredCode)
    {
        if (enteredCode == correctCode)
        {
            Debug.Log("✅ Passwort korrekt!");
            isSolved = true;

            if (energyTower != null)
            {
                foreach (var tower in energyTower)
                {
                    tower.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
                }
            }
            CloseUI();
            shutdownAudio?.Play();
        }
        else
        {
            Debug.Log("❌ Falsches Passwort.");
        }
    }

    public void CloseUI()
    {
        if (passwordUI != null)
        {
            passwordUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public bool IsSolved => isSolved;
    
}

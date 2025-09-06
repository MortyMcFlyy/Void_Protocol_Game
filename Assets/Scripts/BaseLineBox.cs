using UnityEngine;

public class BaseLineBox : MonoBehaviour, IInteractable
{
    public PatrollingLaserEnemy[] baselineEnemy; // Referenz zum PatrollingLaserEnemy-Script
    [SerializeField] private GameObject puzzleUI; // Referenz zum Kabel-Puzzle UI GameObject
    [SerializeField] private AudioSource baseLineAudio;
    private bool isPuzzleOpen = false;

    void Update()
    {
        if (isPuzzleOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    public string GetPrompt()
    {
        return isPuzzleOpen ? "" : "F: Sicherungskasten öffnen";
    }

    public void Interact()
    {
        if (puzzleUI == null)
        {
            Debug.LogError("Puzzle UI wurde nicht zugewiesen!", this);
            return;
        }

        if (!isPuzzleOpen)
        {
            OpenPuzzle();
        }
        else
        {
            ClosePuzzle();
        }
    }

    private void OpenPuzzle()
    {
        puzzleUI.SetActive(true);
        isPuzzleOpen = true;
        // Optional: Cursor sichtbar machen, Spiel pausieren etc.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;  // Pausiert das Spiel
    }

    private void ClosePuzzle()
    {
        puzzleUI.SetActive(false);
        isPuzzleOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;  // Spiel läuft weiter
    }

    // Diese Methode kannst du vom Puzzle-Script aufrufen lassen, wenn das Rätsel gelöst ist
    public void OnPuzzleSolved()
    {
        ClosePuzzle();
        foreach (var enemy in baselineEnemy)
        {
            if (enemy != null)
            {
                enemy.SetBaseLineMode(true);
            }
        }
        baseLineAudio?.Play();
    }
}

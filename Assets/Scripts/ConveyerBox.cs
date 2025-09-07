using UnityEngine;

public class ConveyorBox : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject puzzleUI;
    [SerializeField] private GameObject conveyorBelt;
    [SerializeField] private AudioSource conveyorAudio;
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void ClosePuzzle()
    {
        puzzleUI.SetActive(false);
        isPuzzleOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void OnPuzzleSolved()
    {
        ClosePuzzle();
        Debug.Log("Puzzle gelöst! Stromkreis aktiviert.");
        conveyorAudio?.Play();
        conveyorBelt.GetComponent<ConveyorBelt>().ToggleDirection();
    }
}

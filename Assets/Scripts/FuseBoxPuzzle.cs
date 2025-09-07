using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FuseBoxPuzzle : MonoBehaviour
{
    public Button[] switches;
    [SerializeField] private int[] correctOrder = { 2, 0, 1 };
    private int currentStep = 0;

    public GameObject puzzleUI;
    public GameObject doorToUnlock;

    public UnityEvent onPuzzleSolved;
    public AudioSource doorAudio;


    void Start()
    {
        for (int i = 0; i < switches.Length; i++)
        {
            int index = i;
            switches[i].onClick.AddListener(() => PressSwitch(index));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            puzzleUI.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void PressSwitch(int index)
    {
        if (index == correctOrder[currentStep])
        {
            currentStep++;
            if (currentStep == correctOrder.Length)
            {
                PuzzleSolved();
            }
        }
        else
        {
            currentStep = 0;
        }
    }

    void PuzzleSolved()
    {
        Debug.Log("Rätsel gelöst!");
        puzzleUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (doorToUnlock != null)
        {
            onPuzzleSolved?.Invoke();
            doorAudio?.Play();
        }
    }
}

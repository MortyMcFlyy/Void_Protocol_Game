using UnityEngine;
using UnityEngine.UI;

public class CableConnectionPuzzle : MonoBehaviour
{
    [System.Serializable]
    public struct CablePair
    {
        public Button leftButton;
        public Button rightButton;
        public GameObject cable;  // das Kabel-GameObject, das sichtbar werden soll
        public bool leftPressed;
        public bool rightPressed;
        public bool connected;
    }

    public CablePair[] cablePairs;

    [SerializeField] private GameObject Interactable;
 

    // Wichtige Reihenfolge: erst links drücken, dann rechts
    void Start()
    {
        for (int i = 0; i < cablePairs.Length; i++)
        {
            int index = i; // lokale Kopie für den Listener
            cablePairs[index].cable.SetActive(false); // Kabel anfangs unsichtbar
            cablePairs[index].leftButton.onClick.AddListener(() => OnLeftButtonPressed(index));
            cablePairs[index].rightButton.onClick.AddListener(() => OnRightButtonPressed(index));
        }
    }

    void OnLeftButtonPressed(int index)
    {
        var pair = cablePairs[index];
        if (pair.connected) return;
        pair.leftPressed = true;
        cablePairs[index] = pair; // Wert zurückschreiben, da struct

        Debug.Log(pair.leftButton.name + " links gedrückt");
    }

    void OnRightButtonPressed(int index)
    {
        var pair = cablePairs[index];
        if (pair.connected) return;

        if (pair.leftPressed)
        {
            pair.rightPressed = true;
            pair.connected = true;
            pair.cable.SetActive(true);
            cablePairs[index] = pair; // Wert zurückschreiben
            cablePairs[index].cable.SetActive(true); // Kabel sichtbar machen

            Debug.Log(pair.rightButton.name + " rechts gedrückt, Kabel verbunden!");
            CheckIfPuzzleSolved();
        }
        else
        {
            Debug.Log("Erst linken Knopf drücken!");
        }
    }

    void CheckIfPuzzleSolved()
    {
        foreach(var pair in cablePairs)
        {
            if (!pair.connected) return;
        }
        Debug.Log("Puzzle vollständig gelöst!");
        OnPuzzleSolved();
    }

    void OnPuzzleSolved()
    {
        // Beispiel: Laufband Richtung umdrehen
        Debug.Log("Puzzle gelöst! Laufband umdrehen.");
        Interactable.GetComponent<ConveyorBox>().OnPuzzleSolved();
    }
}

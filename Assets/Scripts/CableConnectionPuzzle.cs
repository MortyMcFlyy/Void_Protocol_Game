using UnityEngine;
using UnityEngine.UI;
using System;


public class CableConnectionPuzzle : MonoBehaviour
{
    [System.Serializable]
    public struct CablePair
    {
        public Button leftButton;
        public Button rightButton;
        public GameObject cable;
        public bool leftPressed;
        public bool rightPressed;
        public bool connected;
    }

    public CablePair[] cablePairs;
    [SerializeField] private string skriptName = "ConveyorBox";
    [SerializeField] private GameObject Interactable;
 

    void Start()
    {
        for (int i = 0; i < cablePairs.Length; i++)
        {
            int index = i;
            cablePairs[index].cable.SetActive(false);
            cablePairs[index].leftButton.onClick.AddListener(() => OnLeftButtonPressed(index));
            cablePairs[index].rightButton.onClick.AddListener(() => OnRightButtonPressed(index));
        }
    }

    void OnLeftButtonPressed(int index)
    {
        var pair = cablePairs[index];
        if (pair.connected) return;
        pair.leftPressed = true;
        cablePairs[index] = pair;

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
            cablePairs[index] = pair;
            cablePairs[index].cable.SetActive(true);

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
        var type = Type.GetType(skriptName);
        if (type != null && Interactable != null)
        {
            var comp = Interactable.GetComponent(type);
            var method = type.GetMethod("OnPuzzleSolved");
            method?.Invoke(comp, null);
        }
        else
        {
            Debug.LogError("Skripttyp nicht gefunden oder Interactable ist null: " + skriptName);
        }
    }
}

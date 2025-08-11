using UnityEngine;

public class ConveyerBox : MonoBehaviour
{
    [SerializeField] private ConveyorBelt belt;

    public void PuzzleSolved()
    {
        
        Debug.Log("Laufband-Richtung geändert!");
    }
}

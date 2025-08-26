using UnityEngine;
using TMPro;

public class PasswordUI : MonoBehaviour
{
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Laptop laptop;

    private string currentInput = "";

    void Start()
    {
        //gameObject.SetActive(false);
    }

    public void AddDigit(string digit)
    {
        if (currentInput.Length < 4)
        {
            currentInput += digit;
            codeDisplay.text = currentInput;
        }

        if (currentInput.Length == 4)
        {
            laptop.CheckCode(currentInput);
            currentInput = ""; // zurücksetzen
            codeDisplay.text = "";
        }
    }

    public void Close()
    {
        laptop.CloseUI();
    }
}

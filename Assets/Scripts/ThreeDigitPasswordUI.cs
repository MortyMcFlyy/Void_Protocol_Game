using UnityEngine;
using TMPro;

public class ThreeDigitPasswordUI : MonoBehaviour
{

    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private PasswordPanel passwordPanel;

    private string currentInput = "";

    public void AddDigit(string digit)
    {
        if (currentInput.Length < 3)
        {
            currentInput += digit;
            codeDisplay.text = currentInput;
        }

        if (currentInput.Length == 3)
        {
            passwordPanel.CheckCode(currentInput);
            currentInput = "";
            codeDisplay.text = "";
        }
    }

    public void Close()
    {
        passwordPanel.CloseUI();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Controls : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseControls()
    {
        Debug.Log("Controls menu closed");
        SceneManager.LoadScene("MainMenu");
    }

}

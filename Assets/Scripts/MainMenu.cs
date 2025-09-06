using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Factory");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }


    public void ShowSettings()
    {
        // Implement settings menu display logic here
        Debug.Log("Settings menu opened");
    }
}

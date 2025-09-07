using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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


    public void ShowControls()
    {
        Debug.Log("Controls menu opened");
        SceneManager.LoadScene("Controls");
    }

    public void MainMenuScene()
    {
        Debug.Log("Main Menu scene loaded");
        SceneManager.LoadScene("MainMenu");
    }

    public void Play()
    {
        Debug.Log("Play button clicked");
        SceneManager.UnloadSceneAsync("Pause");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
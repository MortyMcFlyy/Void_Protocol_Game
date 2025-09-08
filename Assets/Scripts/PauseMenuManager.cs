using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public string pauseMenuSceneName = "Pause";
    private bool isPauseMenuOpen = false;
    public static PauseMenuManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseMenuOpen)
            {
                OpenPauseMenu();
            }
            else
            {
                ClosePauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        if (!isPauseMenuOpen)
        {
            SceneManager.LoadScene(pauseMenuSceneName, LoadSceneMode.Additive);
            isPauseMenuOpen = true;
        }
    }

    public void ClosePauseMenu()
    {
        if (isPauseMenuOpen)
        {
            SceneManager.UnloadSceneAsync(pauseMenuSceneName);
            isPauseMenuOpen = false;
        }
    }

    public void OnPlayButtonPressed()
    {
        ClosePauseMenu();
    }
}
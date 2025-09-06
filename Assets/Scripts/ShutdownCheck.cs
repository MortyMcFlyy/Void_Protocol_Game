using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShutdownCheck : MonoBehaviour
{
    [SerializeField] private PasswordPanel[] passwordPanels;
    [SerializeField] private Transform[] energyTowers;
    [SerializeField] private int numberOfTowers = 1;
    [SerializeField] private bool onOffSwitch = false;
    [SerializeField] private bool audioStarted = false;
    [SerializeField] private AudioSource shutdownAudio;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float waitForEnd = 10f;
    [SerializeField] private bool end = false;

    // Update is called once per frame
    void Update()
    {
        int solvedCount = 0;
        foreach (var panel in passwordPanels)
        {
            if (panel.IsSolved)
            {
                solvedCount++;
            }
        }

        if (solvedCount >= numberOfTowers)
        {
            onOffSwitch = !onOffSwitch;
            // Trigger shutdown sequence
            foreach (var tower in energyTowers)
            {
                tower.GetComponent<Renderer>().material.SetColor("_EmissionColor", onOffSwitch ? Color.red : Color.clear);
            }
            playAudio();
            if (!end)
            {
                StartCoroutine(endGame());
            }
        }
    }

    void playAudio()
    {
        if (audioStarted) return;
        shutdownAudio?.Play();
        audioStarted = true;
    }

    IEnumerator endGame()
    {
        end = true;
        yield return new WaitForSeconds(waitForEnd);
        Debug.Log("Game Over: Shutdown Complete");
        StartCoroutine(playerController.Fade(0f, 1f));
        SwitchToScene("MainMenu");
    }

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

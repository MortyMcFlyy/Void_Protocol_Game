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
    [SerializeField] private AudioSource warningAudio;
    [SerializeField] private PlayerController playerController;
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
        StartCoroutine(PlayWarningAndShutdownAudio());
        audioStarted = true;
    }

    IEnumerator PlayWarningAndShutdownAudio()
{
    if (warningAudio != null)
    {
        warningAudio.Play();
        yield return new WaitForSeconds(warningAudio.clip.length-1f);
        warningAudio.Stop();
    }

    if (shutdownAudio != null)
    {
        shutdownAudio.Play();
    }
}

    IEnumerator endGame()
    {
        end = true;
        yield return new WaitForSeconds(warningAudio.clip.length);
        yield return new WaitForSeconds(shutdownAudio.clip.length / 2 - 2f);
        playerController.currentDeathType = PlayerController.DeathType.Laser;
        StartCoroutine(playerController.PlayDissolveEffect());
        StartCoroutine(playerController.endGameFade(0f, 1f));
        yield return new WaitForSeconds(shutdownAudio.clip.length / 2 + 2f);
        SwitchToScene("GameOver");
    }

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

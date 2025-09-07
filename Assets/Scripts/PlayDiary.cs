using UnityEngine;

public class PlayDiary : MonoBehaviour, IInteractable
{
    public AudioSource diaryAudio;

    public string GetPrompt() => $"F: Tagebuch abspielen";

    public void Interact()
    {
        Debug.Log($"Tagebuch wurde abgespielt.");
        diaryAudio?.Play();
    }
}
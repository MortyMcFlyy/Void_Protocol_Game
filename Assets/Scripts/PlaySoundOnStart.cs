using UnityEngine;

public class PlaySoundOnStart : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource fehlt!");
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class AsynchLoader : MonoBehaviour
{

    [SerializeField] private GameObject LoadingScreenCanvas;
    [SerializeField] private GameObject MenuScreenCanvas;

    [SerializeField] private Slider loadingSlider;

    public void StartGameBtn(string levelToLoad)
    {
        MenuScreenCanvas.SetActive(false);
        LoadingScreenCanvas.SetActive(true);
        StartCoroutine(LoadAsynch(levelToLoad));
    }


    IEnumerator LoadAsynch(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = progress;
            yield return null;
        }



    }
}

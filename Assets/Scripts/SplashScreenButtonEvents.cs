using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenButtonEvents : MonoBehaviour {
    public StateSingleton.StateView sv;
    public Text LoadingObj;

    void Start()
    {
        LoadingObj.gameObject.SetActive(false);
        GetComponent<Button>().onClick.AddListener(ChangeState);
    }

    void ChangeState()
    {
        StateSingleton.stateView = sv;
        StartCoroutine("SceneLoaderWithLoadingBar");
    }

    IEnumerator SceneLoaderWithLoadingBar()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        LoadingObj.gameObject.SetActive(true);
        while (!operation.isDone)
        {
            LoadingObj.text = "Caricamento al " + (operation.progress*100).ToString("00.00") + "%";
            yield return new WaitForEndOfFrame();
        }
    }
}

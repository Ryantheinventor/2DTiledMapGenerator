using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public RectTransform barRect;

    private float barWidth = 0;

    private float progress;
    void Start() 
    {
        barWidth = barRect.sizeDelta.x;
        barRect.anchorMax = new Vector2(0, barRect.anchorMax.y);
        StartCoroutine(LoadScene());    
    }

    public IEnumerator LoadScene()
    {
        int sID = PlayerPrefs.GetInt("TargetScene", 0);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sID, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        while(!asyncLoad.isDone)
        {
            progress = asyncLoad.progress / 2;
            yield return null;
        }
        SceneSpecificLoader ssl = FindObjectOfType<SceneSpecificLoader>();
        Debug.Log(ssl);
        while(!ssl.isDone)
        {
            progress = 0.5f + ssl.progress / 2;
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sID));
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(1));
    }


    void Update() 
    {
        barRect.sizeDelta = new Vector2(progress * barWidth, barRect.sizeDelta.y);    
    }
}

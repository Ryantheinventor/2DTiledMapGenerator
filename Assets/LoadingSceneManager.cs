using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public RectTransform barRect;
    public RectTransform circleRect;
    public Image circleImage;
    public Text text;
    private float barWidth = 0;

    private float progress;
    private float displayProgress = 0;
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
        progress = 1;
        while(displayProgress < 0.98f)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sID));
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(1));
        ssl.OnDone();
    }


    void Update() 
    {
        displayProgress = Mathf.Lerp(displayProgress, progress, 3 * Time.deltaTime);
        barRect.sizeDelta = new Vector2(displayProgress * barWidth, barRect.sizeDelta.y);    
        circleRect.eulerAngles += new Vector3(0,0,-90 * Time.deltaTime);
        circleImage.fillAmount = displayProgress;
        text.text = $"{(int)(displayProgress * 100f)}%";
    }
}

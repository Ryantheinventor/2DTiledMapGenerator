using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLoading : MonoBehaviour
{
    public void LoadLoadingScene(int sceneID)
    {
        PlayerPrefs.SetInt("TargetScene",sceneID);
        SceneManager.LoadScene(1);
    }
}

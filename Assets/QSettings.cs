using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QSettings : MonoBehaviour
{
    private void Start() 
    {
        Screen.SetResolution(1600, 900, FullScreenMode.ExclusiveFullScreen, 200);
        QualitySettings.vSyncCount = 1;
    }
}

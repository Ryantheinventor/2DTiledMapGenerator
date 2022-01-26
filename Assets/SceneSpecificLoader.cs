using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSpecificLoader : MonoBehaviour
{
    public Camera startingCam;

    public MapGenerator mapGenerator;

    [HideInInspector]
    public float progress = 0;

    [HideInInspector]
    public bool isDone = false;

    CoroutineData cd = null;

    void Start() 
    {
        startingCam.enabled = false;
        startingCam.GetComponent<AudioListener>().enabled = false;
        cd = new CoroutineData(this, mapGenerator.GenerateMapAsync(mapGenerator.tileSetData));
    }

    void Update() 
    {

        if(cd!=null)
        {
           MapGenerator.Map theMap = ((MapGenerator.Map)cd.result);
           if(theMap.isDone)
           {
               progress = 1;
           }
           else
           {
               progress = theMap.progress;
           }
           isDone = theMap.isDone;
        }    

    }
    public void OnDone()
    {
        startingCam.GetComponent<AudioListener>().enabled = true;
        startingCam.enabled = true;
        this.enabled = false;
    }

}

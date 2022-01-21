using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aerhsdh : MonoBehaviour
{
    CoroutineData cd;
    void Start() 
    {
        cd = new CoroutineData(this, GetComponent<MapGenerator>().GenerateMapAsync(GetComponent<MapGenerator>().tileSetData, 1000, false));
    }
    void Update() 
    {
        
        Debug.Log(((MapGenerator.Map)cd.result).progress);
        if(((MapGenerator.Map)cd.result).isDone)
        {
            Destroy(this);
        }
    }
}

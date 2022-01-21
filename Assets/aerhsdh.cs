using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aerhsdh : MonoBehaviour
{
    CoroutineData cd = null;
    void Start() 
    {
        
    }
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            cd = new CoroutineData(this, GetComponent<MapGenerator>().GenerateMapAsync(GetComponent<MapGenerator>().tileSetData, 1000, false));
        }
        if(cd != null)
        {
            Debug.Log(((MapGenerator.Map)cd.result).progress);
            if(((MapGenerator.Map)cd.result).isDone)
            {
                Destroy(this);
            }
        }
    
    }
}

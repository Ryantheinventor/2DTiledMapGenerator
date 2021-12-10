using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCounter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GameObject.FindObjectsOfType<TileData>().Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

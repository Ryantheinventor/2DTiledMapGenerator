using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMap : MonoBehaviour
{
    public GameObject miniMap;

    private void Start() 
    {
        miniMap.SetActive(false);
    }

    void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            miniMap.SetActive(true);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public bool pause = false;
    public GameObject fab;
    public float offsetDist = 200;

    private List<GameObject> testedMaps = new List<GameObject>();

    private float testAmount = 1;
    private float waitTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        Test();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!pause && waitTime <= 0)
        {
            Test();           
            waitTime = 2; 
        }
        else if(waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
    }

    private void Test()
    {
        Clean();
        for(int i = 0; i < testAmount; i++)
        {
            testedMaps.Add(Instantiate(fab));
            
            testedMaps[i].GetComponent<MapGenerator>().GenerateMap(testedMaps[i].GetComponent<MapGenerator>().tileSetData, 
                                                                   out List<MapGenerator.DoorLocation> openDoors, 
                                                                   out List<MapGenerator.DoorLocation> unlockedDoors, 
                                                                   out List<MapGenerator.DoorLocation> lockedDoors, 
                                                                   out List<MapGenerator.PlacedRoomData> placedRooms, 
                                                                   true);
            testedMaps[i].transform.position = new Vector3(offsetDist * i,0,0);
            testedMaps[i].SetActive(false);
        }
        foreach(GameObject g in testedMaps)
        {
            g.SetActive(true);
        }
    }

    private void Clean()
    {
        foreach(GameObject g in testedMaps)
        {
            g.SetActive(false);
            Destroy(g);
        }
        testedMaps = new List<GameObject>();
    }

}

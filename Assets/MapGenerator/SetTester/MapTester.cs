using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTester : MonoBehaviour
{
    public int attemptsToMake = 100;
    public RoomTileMap setToTest;
    public GameObject prefab;
    private GameObject genObject;

    private int failedAttempts = 0;
    private int attempts = 0;
    private bool done = false;
    private bool lastFailed = false;
    void Update() 
    {
        if(attempts < attemptsToMake)
        {
            if(genObject && !lastFailed)
            {
                Destroy(genObject);
            }
            else if(genObject)
            {
                genObject.name = "MapTest Fail " + failedAttempts;
                genObject.SetActive(false);
                lastFailed = false;
            }
            genObject = Instantiate(prefab);
            MapGenerator mapGen = genObject.GetComponent<MapGenerator>();
            if(!mapGen.GenerateMap(setToTest, 
                            out List<MapGenerator.DoorLocation> doorways, 
                            out List<MapGenerator.PlacedRoomData> placedRooms,
                            1, 
                            false))
            {
                
                lastFailed = true;
                failedAttempts++;
            }
            int lCount = 0;
            foreach(MapGenerator.DoorLocation d in doorways)
            {
                if(d.isLocked)
                {
                    lCount++;
                }
            }    
            Debug.Log($"Total Doors:{doorways.Count}, Locked Doors:{lCount}, Rooms:{placedRooms.Count}");
            attempts++;

        }
        else
        {
            if(!done)
            {
                Debug.Log(setToTest.name + "results:" + failedAttempts + "/" + attempts + " Failed. The faild attempts have been left in the scene deactivated for review.");
                done = true;
            }
        }
    }
}
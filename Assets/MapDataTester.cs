using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTMG;
using static RTMG.MapGenerator;
public class MapDataTester : MonoBehaviour
{
    void Update()
    {
    }

    void OnDrawGizmos() 
    {
        Map theMap = GetComponent<SceneSpecificLoader>().map;
        if(theMap != null)
        {  
            foreach(DoorLocation dl in theMap.doorways)
            {
                int i = 0;
                foreach(PlacedRoomData pr in dl.myRooms)
                {
                    Gizmos.DrawLine(new Vector3(dl.position.x, 10 + i, dl.position.y),new Vector3(pr.position.x, 10 + i, pr.position.y));
                    i++;
                }
                
            }
            Gizmos.color = Color.red;
            foreach(PlacedRoomData pr in theMap.rooms)
            {
                int i = 0;
                foreach(DoorLocation dl in pr.myDoors)
                {
                    Gizmos.DrawLine(new Vector3(pr.position.x, 5 + i, pr.position.y),new Vector3(dl.position.x, 5 + i, dl.position.y));
                    i++;
                }
                
            }
            Gizmos.color = Color.green;
            foreach(PlacedRoomData pr in theMap.rooms)
            {
                int i = 0;
                foreach(PlacedRoomData dl in pr.adjacentRooms)
                {
                    Gizmos.DrawLine(new Vector3(pr.position.x, 15 + i, pr.position.y),new Vector3(dl.position.x, 16 + i, dl.position.y));
                    i++;
                }
                
            }
        }
    }
}


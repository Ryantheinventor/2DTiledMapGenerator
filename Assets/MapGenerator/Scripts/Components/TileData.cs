using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileData : MonoBehaviour
{
    public List<DoorValues> doorPositions = new List<DoorValues>();
    
    [Serializable]
    public struct DoorValues
    {
        public Vector2 position;
        [Tooltip("0 Degrees on a non rotated tile will always point to the right")]
        public float rotation;
    }

    public Vector2 tileSize = new Vector2(5,5);

    [HideInInspector]
    public List<bool> doorsTaken = new List<bool>();

    public RoomTileMap.AxisMode gizmoAxisMode = RoomTileMap.AxisMode.XY;

    void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        foreach(DoorValues v in doorPositions)
        {
            switch(gizmoAxisMode)
            {
                case RoomTileMap.AxisMode.XY:
                {
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,0,transform.eulerAngles.z) * new Vector3(v.position.x,v.position.y,0));
                    Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f)); 
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+v.rotation) * new Vector3(1,0,0))); 
                    break;
                }
                case RoomTileMap.AxisMode.XZ:
                {    
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,transform.eulerAngles.y,0) * new Vector3(v.position.x,0,v.position.y));
                    Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f));  
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,transform.eulerAngles.y+v.rotation,0) * new Vector3(1,0,0))); 
                    break;
                }
            }
        }

          
    }

}

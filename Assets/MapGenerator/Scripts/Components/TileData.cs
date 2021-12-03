using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour
{
    public List<Vector2> doorPositions = new List<Vector2>();
    
    public Vector2 tileSize = new Vector2(5,5);

    [HideInInspector]
    public List<bool> doorsTaken = new List<bool>();

    public RoomTileMap.AxisMode gizmoAxisMode = RoomTileMap.AxisMode.XY;

    void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        foreach(Vector2 v in doorPositions)
        {
            switch(gizmoAxisMode)
            {
                case RoomTileMap.AxisMode.XY:
                    Gizmos.DrawCube(transform.position + (Quaternion.Euler(0,0,transform.eulerAngles.z) * new Vector3(v.x,v.y,0)), new Vector3(0.2f,0.2f,0.2f));  
                    break;
                case RoomTileMap.AxisMode.XZ:
                    Gizmos.DrawCube(transform.position + (Quaternion.Euler(0,transform.eulerAngles.y,0) * new Vector3(v.x,0,v.y)), new Vector3(0.2f,0.2f,0.2f));  
                    break;
            }
        }

          
    }

}

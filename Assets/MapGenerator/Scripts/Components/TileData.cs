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


    //TODO make this part look better in the inspector

    public List<Collider> colliders3D = new List<Collider>();
    public List<Collider2D> colliders2D = new List<Collider2D>();

    void OnInspectorUpdate() 
    {
        if(colliders2D.Count > 0 && colliders3D.Count > 0)
            Debug.LogError($"Collider type mix detected in TileData on:({gameObject.name}), please only use one type of collider to check for room overlapping, for now only 2D will be used.");   
    }


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

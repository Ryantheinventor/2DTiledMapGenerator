using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

[CreateAssetMenu(fileName = "RoomTileMap", menuName = "RandomMapGenerator/RoomTileMap", order = 0)]
public class RoomTileMap : ScriptableObject 
{
    [HideInInspector]
    public List<TileWithState> startTiles = new List<TileWithState>();

    [HideInInspector]
    public List<TileWithState> endTiles = new List<TileWithState>();

    [HideInInspector]
    public List<TileWithState> doorCaps = new List<TileWithState>();

    [HideInInspector]
    public List<TileWithState> tileSet = new List<TileWithState>();

    [HideInInspector]
    public AxisMode axisMode = AxisMode.XY;

    [HideInInspector]
    public bool allowRotation = false;

    [HideInInspector]
    public int maxRoomDistance = 10;

    [HideInInspector]
    public bool useRB2D = true;

    [Serializable]
    public struct TileWithState
    {
        public GameObject tileObject;
        public bool tileUsable;
        public Texture2D previewImage;
        public float rarity;
    }

    [Serializable]
    public enum AxisMode
    {
        XY,//2D
        XZ//3D
    }


    
}

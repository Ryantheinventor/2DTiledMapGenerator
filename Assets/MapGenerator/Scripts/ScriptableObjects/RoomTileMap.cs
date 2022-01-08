using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

[CreateAssetMenu(fileName = "RoomTileMap", menuName = "RandomMapGenerator/RoomTileMap", order = 0)]
public class RoomTileMap : ScriptableObject 
{
    
    public List<TileWithState> startTiles = new List<TileWithState>();

    
    public List<TileWithState> endTiles = new List<TileWithState>();

    
    public List<TileWithState> doorCaps = new List<TileWithState>();

    
    public List<TileWithState> tileSet = new List<TileWithState>();

    
    public AxisMode axisMode = AxisMode.IS2D;

    
    public bool allowRotation = false;

    
    public int maxRoomDistance = 10;

    
    public bool useRB2D = true;

    public float maxOverlap = 0.1f;

    public int maxTileCount = 50;

    public int minTileCount = 20;

    [Serializable]
    public struct TileWithState
    {
        public GameObject tileObject;
        public bool tileUsable;
        public Texture2D previewImage;
        public float rarity;
        public int requiredUse;
    }

    [Serializable]
    public enum AxisMode
    {
        IS2D,//2D
        IS3D//3D
    }


    
}
